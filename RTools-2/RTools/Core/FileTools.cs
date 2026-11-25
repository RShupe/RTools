using System.Text;

namespace RTools.Core;

public record JoinResult(int Files, long Characters, IReadOnlyList<string> DeleteFailures);
public record SearchResult(long Matched, long Unmatched, string? Backup);

public static class FileTools
{
    public static readonly UTF8Encoding Utf8 = new(false, true);
    public static StringComparer PathComparer => StringComparer.OrdinalIgnoreCase;

    public static string Input(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Select an input file.");
        var full = Path.GetFullPath(path);
        if (!File.Exists(full)) throw new FileNotFoundException("Input file not found.", full);
        return full;
    }

    public static string Output(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Select an output file.");
        var full = Path.GetFullPath(path);
        if (!Directory.Exists(Path.GetDirectoryName(full))) throw new DirectoryNotFoundException("The output folder does not exist.");
        if (Directory.Exists(full)) throw new IOException("The output path is a folder.");
        return full;
    }

    private static string ResolvePath(string path)
    {
        return WindowsPaths.Resolve(path);
    }

    public static void EnsureDifferent(string source, string target)
    {
        if (PathComparer.Equals(Path.GetFullPath(source), Path.GetFullPath(target)) || PathComparer.Equals(ResolvePath(source), ResolvePath(target)) || WindowsPaths.SameFile(source, target))
            throw new ArgumentException("Input and output must be different files.");
    }

    internal static string TempFor(string path) => Path.Combine(Path.GetDirectoryName(path)!, $".rtools-{Guid.NewGuid():N}.tmp");
    internal static void Commit(string temp, string destination)
    {
        // Same-directory staging. Replace existing files atomically without deleting them first.
        if (File.Exists(destination)) File.Replace(temp, destination, null);
        else File.Move(temp, destination);
    }
    internal static void Cleanup(string? path) { if (path is not null) try { File.Delete(path); } catch { /* best effort */ } }
    internal static StreamReader OpenText(string path) => new(path, Utf8, true, 65536);
    internal static StreamWriter NewText(string path, Encoding? encoding = null) => new(new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536), encoding ?? Utf8, 65536);

    public static JoinResult Join(IEnumerable<string> files, string output, int blankLines, bool deleteOriginals,
        CancellationToken token = default, IProgress<string>? progress = null)
    {
        if (blankLines is < 0 or > 1000) throw new ArgumentOutOfRangeException(nameof(blankLines), "Choose between 0 and 1000 blank lines.");
        var inputs = files.Select(Input).Distinct(PathComparer).ToArray();
        if (inputs.Length == 0) throw new ArgumentException("Add at least one file.");
        output = Output(output);
        foreach (var input in inputs) EnsureDifferent(input, output);
        var temp = TempFor(output);
        long chars = 0;
        try
        {
            using (var writer = NewText(temp))
            {
                var buffer = new char[65536];
                for (var i = 0; i < inputs.Length; i++)
                {
                    token.ThrowIfCancellationRequested();
                    progress?.Report($"Joining file {i + 1} of {inputs.Length}...");
                    if (i > 0) for (var j = 0; j <= blankLines; j++) writer.WriteLine();
                    using var reader = OpenText(inputs[i]);
                    int count;
                    while ((count = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        writer.Write(buffer, 0, count);
                        chars += count;
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            Commit(temp, output);
        }
        finally { Cleanup(temp); }
        // The output is committed before deleting anything. Cancellation is no longer honored during commit/cleanup.
        var failures = new List<string>();
        if (deleteOriginals) foreach (var input in inputs)
        {
            try { File.Delete(input); } catch (Exception ex) { failures.Add($"{input}: {ex.Message}"); }
        }
        return new(inputs.Length, chars, failures);
    }

    private static string[] Keywords(IEnumerable<string> keywords) => keywords.Where(k => !string.IsNullOrWhiteSpace(k)).Distinct(StringComparer.Ordinal).ToArray();
    private static bool IsMatch(string line, string[] keywords, bool ignoreCase) => keywords.Any(k => line.Contains(k, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));

    public static SearchResult PreviewSearch(string source, IEnumerable<string> keywords, bool ignoreCase, CancellationToken token = default)
    {
        var terms = Keywords(keywords);
        if (terms.Length == 0) throw new ArgumentException("Enter at least one keyword.");
        long matched = 0, unmatched = 0;
        using var reader = OpenText(Input(source));
        while (reader.ReadLine() is { } line)
        {
            token.ThrowIfCancellationRequested();
            if (IsMatch(line, terms, ignoreCase)) matched++; else unmatched++;
        }
        return new(matched, unmatched, null);
    }

    // Existing destination content is copied to staging before appending; the source is staged separately.
    public static SearchResult SearchAppend(string source, string destination, IEnumerable<string> keywords, bool ignoreCase, bool move,
        CancellationToken token = default, IProgress<string>? progress = null)
    {
        source = Input(source); destination = Output(destination); EnsureDifferent(source, destination);
        var terms = Keywords(keywords);
        if (terms.Length == 0) throw new ArgumentException("Enter at least one keyword.");
        var targetTemp = TempFor(destination);
        string? sourceTemp = move ? TempFor(source) : null;
        string? backup = null;
        long matched = 0, unmatched = 0;
        bool committed = false;
        try
        {
            using (var reader = OpenText(source))
            {
                reader.Peek(); // Detect BOM before opening the staged source.
                using var remaining = sourceTemp is null ? null : NewText(sourceTemp, reader.CurrentEncoding);
                using var target = NewText(targetTemp);
                // Preserve destination text, normalize its encoding to UTF-8, and separate the first appended line.
                char last = '\n'; bool hadContent = false;
                if (File.Exists(destination))
                {
                    using var previous = OpenText(destination);
                    var buffer = new char[65536]; int n;
                    while ((n = previous.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        token.ThrowIfCancellationRequested(); target.Write(buffer, 0, n); last = buffer[n - 1]; hadContent = true;
                    }
                }
                while (reader.ReadLine() is { } line)
                {
                    token.ThrowIfCancellationRequested();
                    if (IsMatch(line, terms, ignoreCase))
                    {
                        if (matched == 0 && hadContent && last is not '\n' and not '\r') target.WriteLine();
                        target.WriteLine(line); matched++;
                    }
                    else { remaining?.WriteLine(line); unmatched++; }
                    if ((matched + unmatched) % 25000 == 0) progress?.Report($"Scanned {matched + unmatched:N0} lines...");
                }
            }
            token.ThrowIfCancellationRequested();
            if (matched == 0) return new(0, unmatched, null);
            if (move)
            {
                backup = source + $".rtools-{Guid.NewGuid():N}.bak";
                File.Copy(source, backup, false);
            }
            token.ThrowIfCancellationRequested();
            Commit(targetTemp, destination); committed = true;
            if (sourceTemp is not null) Commit(sourceTemp, source);
            return new(matched, unmatched, backup);
        }
        catch (Exception ex) when (committed)
        {
            throw new IOException($"Matching lines were written to the destination, but the source could not be replaced. Do not retry until you review both files. Source backup: {backup}", ex);
        }
        finally { Cleanup(targetTemp); Cleanup(sourceTemp); if (!committed) Cleanup(backup); }
    }
}
