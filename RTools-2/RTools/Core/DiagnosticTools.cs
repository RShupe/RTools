using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace RTools.Core;

public sealed record SubnetResult(string Address, int Prefix, string Mask, string Wildcard, string Network, string Broadcast, string FirstHost, string LastHost, ulong Total, ulong Usable, string Binary);
public static class SubnetCalculator
{
    public static uint ParseAddress(string value)
    {
        var parts = value.Trim().Split('.'); uint result = 0;
        if (parts.Length != 4) throw new ArgumentException("Enter four decimal IPv4 octets.");
        foreach (var p in parts)
        {
            if (p.Length is < 1 or > 3 || p.Any(c => c < '0' || c > '9') || !byte.TryParse(p, NumberStyles.None, CultureInfo.InvariantCulture, out byte n)) throw new ArgumentException("Each IPv4 octet must be 0–255.");
            result = (result << 8) | n;
        }
        return result;
    }
    public static string Format(uint value) => $"{value >> 24}.{(value >> 16) & 255}.{(value >> 8) & 255}.{value & 255}";
    public static SubnetResult Calculate(string address, string maskOrPrefix)
    {
        var parts = address.Trim().Split('/'); if (parts.Length > 2) throw new ArgumentException("Invalid CIDR address.");
        uint ip = ParseAddress(parts[0]); var input = (parts.Length == 2 ? parts[1] : maskOrPrefix).Trim().TrimStart('/');
        int prefix; uint mask;
        if (input.Contains('.'))
        {
            mask = ParseAddress(input); uint inverse = ~mask;
            if (((ulong)inverse & ((ulong)inverse + 1)) != 0) throw new ArgumentException("Subnet mask must have contiguous leading ones.");
            prefix = Compat.PopCount(mask);
        }
        else
        {
            if (!int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out prefix) || prefix < 0 || prefix > 32) throw new ArgumentException("Prefix must be 0–32 or a contiguous dotted subnet mask.");
            mask = prefix == 0 ? 0 : uint.MaxValue << (32 - prefix);
        }
        uint network = ip & mask, broadcast = network | ~mask; ulong total = 1UL << (32 - prefix);
        uint first = prefix >= 31 ? network : network + 1, last = prefix >= 31 ? broadcast : broadcast - 1;
        return new(Format(ip), prefix, Format(mask), Format(~mask), Format(network), Format(broadcast), Format(first), Format(last), total, prefix >= 31 ? total : total - 2, string.Join(".", Format(mask).Split('.').Select(p => Convert.ToString(int.Parse(p, CultureInfo.InvariantCulture), 2).PadLeft(8, '0'))));
    }
}

public sealed record LogLine(int Number, string Severity, string Text);
public sealed record LogSnapshot(IReadOnlyList<LogLine> Lines, long Bytes, int Scanned, bool Partial);
public static class LogReader
{
    // Read only the final 1 MiB; a shared handle allows active log writers to continue.
    public static LogSnapshot Read(string path, string keyword, string severity, int maxLines, bool ignoreCase, CancellationToken token = default)
    {
        if (maxLines is < 1 or > 5000) throw new ArgumentException("Choose 1–5000 lines.");
        using var stream = new FileStream(FileTools.Input(path), FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        long length = stream.Length; int size = (int)Math.Min(length, 1024 * 1024); stream.Seek(length - size, SeekOrigin.Begin);
        var bytes = new byte[size]; int read = 0, n;
        while (read < size && (n = stream.Read(bytes, read, size - read)) > 0) { token.ThrowIfCancellationRequested(); read += n; }
        string text;
        if (length == size) { using var buffer = new MemoryStream(bytes, 0, read); using var decoder = new StreamReader(buffer, Encoding.UTF8, true); text = decoder.ReadToEnd(); }
        else
        {
            // Large logs are expected to be UTF-8. Drop the first partial line at the seek boundary.
            int start = Array.IndexOf(bytes, (byte)'\n', 0, read); text = start < 0 ? "" : Encoding.UTF8.GetString(bytes, start + 1, read - start - 1);
        }
        var lines = new Queue<LogLine>(); using var reader = new StringReader(text); int count = 0; string? line;
        while ((line = reader.ReadLine()) != null)
        {
            token.ThrowIfCancellationRequested(); count++;
            if (line.Length > 16000) line = line.Substring(0, 16000) + " … [truncated]";
            var level = Severity(line);
            if ((severity == "All" || level == severity) && line.Contains(keyword, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            { lines.Enqueue(new(count, level, line)); if (lines.Count > maxLines) lines.Dequeue(); }
        }
        return new(lines.ToArray(), length, count, length > size);
    }
    public static string Severity(string line)
    {
        if (Regex.IsMatch(line, @"\b(FATAL|CRITICAL|ERROR|ERR|EXCEPTION)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100))) return "Error";
        if (Regex.IsMatch(line, @"\b(WARN|WARNING)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100))) return "Warning";
        if (Regex.IsMatch(line, @"\b(INFO|INFORMATION)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100))) return "Info";
        if (Regex.IsMatch(line, @"\b(DEBUG|TRACE)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100))) return "Debug";
        return "Other";
    }
}

public sealed record RegexHit(int Index, int Length, string Value, string Groups);
public sealed record RegexResult(IReadOnlyList<RegexHit> Hits, string? Replacement, bool Truncated);
public static class RegexWorkbench
{
    public static RegexResult Run(string text, string pattern, string replacement, RegexOptions options, bool replace, CancellationToken token = default)
    {
        if (text.Length > 200_000 || pattern.Length > 2000 || replacement.Length > 10_000) throw new ArgumentException("Limits: 200,000 text characters, 2,000 pattern characters, 10,000 replacement characters.");
        var regex = new Regex(pattern, options | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(300));
        if (replace && (long)text.Length * replacement.Length > 20_000_000) throw new ArgumentException("Replacement preview may be too large; shorten the sample or replacement.");
        var groupNames = regex.GetGroupNames().Where(g => g != "0").Take(30).ToArray();
        var hits = new List<RegexHit>(); var watch = System.Diagnostics.Stopwatch.StartNew(); bool truncated = false;
        var result = replace ? new StringBuilder() : null; int position = 0;
        foreach (Match match in regex.Matches(text))
        {
            token.ThrowIfCancellationRequested();
            if (watch.Elapsed > TimeSpan.FromSeconds(3)) throw new TimeoutException("Regex exceeded the 3-second work budget.");
            if (hits.Count >= 1000) { truncated = true; break; }
            string groups = string.Join(" · ", groupNames.Select(g => $"{g}: {(match.Groups[g].Success ? Short(match.Groups[g].Value) : "(no match)")}"));
            hits.Add(new(match.Index, match.Length, Short(match.Value), groups));
            if (result is not null)
            {
                result.Append(text, position, match.Index - position); result.Append(match.Result(replacement)); position = match.Index + match.Length;
                if (result.Length > 1_000_000) throw new InvalidDataException("Replacement output exceeds 1 million characters.");
            }
        }
        if (result is not null && !truncated) { result.Append(text, position, text.Length - position); if (result.Length > 1_000_000) throw new InvalidDataException("Replacement output exceeds 1 million characters."); }
        return new(hits, truncated ? null : result?.ToString(), truncated);
    }
    private static string Short(string value) => value.Length > 500 ? value.Substring(0, 500) + "…" : value;
}

public sealed record Rgb(byte R, byte G, byte B)
{
    public string Hex => $"#{R:X2}{G:X2}{B:X2}";
    public string Css => $"rgb({R}, {G}, {B})";
    public (double H, double S, double L) Hsl
    {
        get
        {
            double r = R / 255d, g = G / 255d, b = B / 255d; double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b)), d = max - min, l = (max + min) / 2;
            if (d == 0) return (0, 0, l * 100);
            double h = max == r ? (g - b) / d % 6 : max == g ? (b - r) / d + 2 : (r - g) / d + 4;
            return ((h * 60 + 360) % 360, d / (1 - Math.Abs(2 * l - 1)) * 100, l * 100);
        }
    }
    public string HslCss { get { var (h,s,l) = Hsl; return FormattableString.Invariant($"hsl({h:F1}, {s:F1}%, {l:F1}%)"); } }
    public static Rgb Parse(string text)
    {
        var value = text.Trim().TrimStart('#'); if (value.Length == 3) value = string.Concat(value.Select(c => new string(c, 2)));
        if (value.Length != 6 || !uint.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint n)) throw new ArgumentException("Enter a 3- or 6-digit HEX color, such as #F83357.");
        return new((byte)(n >> 16), (byte)(n >> 8), (byte)n);
    }
    public static Rgb FromHsl(double hue, double saturation, double lightness)
    {
        if (!Compat.IsFinite(hue) || !Compat.IsFinite(saturation) || !Compat.IsFinite(lightness) || hue < 0 || hue > 360 || saturation < 0 || saturation > 100 || lightness < 0 || lightness > 100) throw new ArgumentException("H: 0–360, S and L: 0–100.");
        double h = hue % 360 / 60, s = saturation / 100, l = lightness / 100, c = (1 - Math.Abs(2 * l - 1)) * s, x = c * (1 - Math.Abs(h % 2 - 1)), m = l - c / 2;
        var (r,g,b) = (int)h switch { 0 => (c,x,0d), 1 => (x,c,0d), 2 => (0d,c,x), 3 => (0d,x,c), 4 => (x,0d,c), _ => (c,0d,x) };
        byte Byte(double v) => (byte)Math.Max(0, Math.Min(255, Math.Round((v + m) * 255)));
        return new(Byte(r), Byte(g), Byte(b));
    }
}
