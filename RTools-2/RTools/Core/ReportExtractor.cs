using System.Text;
using System.Text.RegularExpressions;

namespace RTools.Core;

public record ReportOptions(int HeaderLines = 11, string DebitPrefix = "485582", string OtherPrefixes = "424447,568513", bool MaskIdentifiers = true);
public record ReportResult(long Exportable, long Ignored, long OtherCards, long Malformed);

public static class ReportExtractor
{
    public static readonly string[] Headers = ["CARD NUMBER", "ACCOUNT", "DATE TIME", "DESCR TYP CODE", "AMOUNT", "MERCH ID", "MCC", "LOCATION", "COMPANY", "CARD STATUS"];
    public static readonly string[] IgnoredCodes = ["WDL-CHK", "INQ-SAV", "INQ-CHK", "MTC-CHK", "WDL-CCR", "WDL-SAV", "INQ-CCR", "TFR-CHK", "MTF-CHK"];
    private const int MinLength = 420;
    private const int MaxRecordLength = 2_000_000;

    // Preserve line terminators because the legacy report uses character offsets, not columns within a line.
    private static IEnumerable<string> RawLines(StreamReader reader, CancellationToken token)
    {
        var line = new StringBuilder();
        var buffer = new char[65536]; int n;
        while ((n = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            token.ThrowIfCancellationRequested();
            for (int i = 0; i < n; i++)
            {
                char c = buffer[i];
                line.Append(c);
                if (c == '\n') { yield return line.ToString(); line.Clear(); }
                else if (line.Length > MaxRecordLength) throw new InvalidDataException("A report line is too large. Check the input format.");
            }
        }
        if (line.Length > 0) yield return line.ToString();
    }

    private static Regex Boundary(ReportOptions options)
    {
        if (options.HeaderLines is < 0 or > 10000) throw new ArgumentException("Header lines must be between 0 and 10000.");
        var prefixes = new[] { options.DebitPrefix }.Concat(options.OtherPrefixes.Split(',').Select(p => p.Trim()).Where(p => p.Length != 0)).Distinct().ToArray();
        if (prefixes.Any(p => !Regex.IsMatch(p, "^[0-9]{6}$"))) throw new ArgumentException("Each card prefix must contain exactly six digits.");
        return new Regex(@"(?<![0-9])(?:" + string.Join("|", prefixes) + @")[0-9]{10}(?![0-9])", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2));
    }

    public static ReportResult Preview(string input, ReportOptions options, CancellationToken token = default, IProgress<string>? progress = null)
        => Scan(FileTools.Input(input), options, null, token, progress);

    public static ReportResult Export(string input, string output, ReportOptions options, CancellationToken token = default, IProgress<string>? progress = null)
    {
        input = FileTools.Input(input); output = FileTools.Output(output); FileTools.EnsureDifferent(input, output);
        var temp = FileTools.TempFor(output);
        try
        {
            ReportResult result;
            using (var writer = FileTools.NewText(temp))
            {
                writer.WriteLine(string.Join(",", Headers.Select(EscapeCsv)));
                result = Scan(input, options, writer, token, progress);
            }
            if (result.Exportable == 0) throw new InvalidDataException("No exportable debit records were found. Check the report format, prefixes and header count. The output was not changed.");
            token.ThrowIfCancellationRequested();
            FileTools.Commit(temp, output);
            return result;
        }
        finally { FileTools.Cleanup(temp); }
    }

    private static ReportResult Scan(string input, ReportOptions options, StreamWriter? writer, CancellationToken token, IProgress<string>? progress)
    {
        var boundary = Boundary(options);
        long exported = 0, ignored = 0, other = 0, malformed = 0;
        var record = new StringBuilder(); bool inRecord = false;
        void Finish()
        {
            if (!inRecord) return;
            token.ThrowIfCancellationRequested();
            var text = record.ToString(); record.Clear();
            if (!text.StartsWith(options.DebitPrefix, StringComparison.Ordinal)) { other++; return; }
            if (IgnoredCodes.Any(c => text.Contains(c, StringComparison.Ordinal))) { ignored++; return; }
            if (text.Length < MinLength) { malformed++; return; }
            string Part(int start, int length) => text.Substring(start, length).Trim();
            var card = Part(0, 16); var account = Part(145, 10); var mcc = Part(415, 5);
            if (mcc.EndsWith('P')) mcc = Part(413, 5);
            if (options.MaskIdentifiers) { card = Mask(card); account = Mask(account); }
            string[] fields = [card, account, Part(23, 12), Part(74, 10), Part(91, 14).Replace(",", ""), Part(392, 16), mcc, Part(207, 18), Part(224, 21), Part(314, 41).TrimEnd('=').TrimEnd()];
            if (writer is not null) writer.WriteLine(string.Join(",", fields.Select((value, i) => EscapeCsv(Neutralize(value, i == 4)))));
            exported++;
            if (exported % 1000 == 0) progress?.Report($"Processed {exported:N0} exportable records...");
        }
        void Append(string line, int start, int count)
        {
            if (!inRecord || count == 0) return;
            if (record.Length + count > MaxRecordLength) throw new InvalidDataException("A report record exceeded the size limit. Check the card prefixes or file format.");
            record.Append(line, start, count);
        }
        using var reader = FileTools.OpenText(input);
        int skipped = 0;
        foreach (var line in RawLines(reader, token))
        {
            if (skipped < options.HeaderLines) { skipped++; continue; }
            int position = 0;
            foreach (Match match in boundary.Matches(line))
            {
                Append(line, position, match.Index - position);
                Finish(); inRecord = true; position = match.Index;
            }
            Append(line, position, line.Length - position);
        }
        Finish(); // Legacy implementation lost the last record at EOF.
        return new(exported, ignored, other, malformed);
    }

    private static string Mask(string value) => value.Length <= 4 ? new string('*', value.Length) : new string('*', value.Length - 4) + value.Substring(value.Length - 4);
    // Avoid Excel formula injection; a signed numeric amount is safe as a numeric literal.
    private static string Neutralize(string value, bool amount)
    {
        if (value.Length == 0) return value;
        var trimmed = value.TrimStart();
        if (trimmed.Length > 0 && "=+-@".Contains(trimmed[0]) && !(amount && Regex.IsMatch(trimmed, @"^[+-]?[0-9]+(?:\.[0-9]+)?$"))) return "'" + value;
        return value;
    }
    public static string EscapeCsv(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
}
