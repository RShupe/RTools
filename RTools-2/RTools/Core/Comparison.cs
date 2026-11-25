using System.Text;

namespace RTools.Core;

public sealed record DiffRow(int? LeftLine, string Left, int? RightLine, string Right, string Kind);
public sealed record DiffResult(IReadOnlyList<DiffRow> Rows, int Added, int Removed, int Changed);
public sealed record BinaryResult(long LeftBytes, long RightBytes, long? FirstDifference);

public static class Comparison
{
    // Bounds keep the desktop responsive even on adversarial or unrelated files.
    public const int MaxTextLines = 20_000;
    public const int MaxTextChars = 2_000_000;
    private const long MaxMatrixCells = 8_000_000;

    public static BinaryResult Binary(string left, string right, CancellationToken token = default)
    {
        using var a = File.OpenRead(FileTools.Input(left));
        using var b = File.OpenRead(FileTools.Input(right));
        var al = a.Length; var bl = b.Length;
        var ab = new byte[65536]; var bb = new byte[65536]; long offset = 0;
        while (offset < Math.Min(al, bl))
        {
            token.ThrowIfCancellationRequested();
            int n = (int)Math.Min(ab.Length, Math.Min(al, bl) - offset);
            Compat.ReadExactly(a, ab, n); Compat.ReadExactly(b, bb, n);
            for (int i = 0; i < n; i++) if (ab[i] != bb[i]) return new(al, bl, offset + i);
            offset += n;
        }
        return new(al, bl, al == bl ? null : offset);
    }
    public static string[] ReadLines(string path, CancellationToken token = default)
    {
        path = FileTools.Input(path);
        if (new FileInfo(path).Length > 8_000_000) throw new InvalidDataException("Text comparison supports files up to 8 MB. Use binary comparison for larger files.");
        using var reader = FileTools.OpenText(path); var lines = new List<string>(); int chars = 0;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            token.ThrowIfCancellationRequested(); chars += line.Length;
            if (line.Contains('\0')) throw new InvalidDataException("The file appears to be binary. Select binary comparison.");
            if (lines.Count >= MaxTextLines || chars > MaxTextChars) throw new InvalidDataException("Text comparison limit: 20,000 lines and 2 million characters per file.");
            lines.Add(line);
        }
        return lines.ToArray();
    }
    public static DiffResult Text(string left, string right, bool ignoreCase, bool ignoreWhitespace, CancellationToken token = default) =>
        CompareLines(ReadLines(left, token), ReadLines(right, token), ignoreCase, ignoreWhitespace, token);

    public static DiffResult CompareLines(string[] a, string[] b, bool ignoreCase = false, bool ignoreWhitespace = false, CancellationToken token = default)
    {
        if (a.Length > MaxTextLines || b.Length > MaxTextLines) throw new ArgumentException("Too many lines.");
        string Normalize(string s) => ignoreWhitespace ? string.Concat(s.Where(c => !char.IsWhiteSpace(c))) : s;
        var x = a.Select(Normalize).ToArray(); var y = b.Select(Normalize).ToArray();
        var cmp = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        bool Same(int i, int j) => cmp.Equals(x[i], y[j]);
        var rows = new List<DiffRow>(); int added = 0, removed = 0, changed = 0;
        void Equal(int i, int j) => rows.Add(new(i + 1, a[i], j + 1, b[j], "Equal"));
        int pre = 0, suffix = 0;
        while (pre < a.Length && pre < b.Length && Same(pre, pre)) { token.ThrowIfCancellationRequested(); Equal(pre, pre); pre++; }
        while (suffix < a.Length - pre && suffix < b.Length - pre && Same(a.Length - suffix - 1, b.Length - suffix - 1)) suffix++;
        int n = a.Length - pre - suffix, m = b.Length - pre - suffix;
        if ((long)(n + 1) * (m + 1) > MaxMatrixCells) throw new InvalidDataException("The changed region is too large for an exact text diff. Compare smaller extracts or select binary comparison.");
        var lcs = new int[n + 1, m + 1];
        for (int i = n - 1; i >= 0; i--)
        {
            token.ThrowIfCancellationRequested();
            for (int j = m - 1; j >= 0; j--) lcs[i, j] = Same(pre + i, pre + j) ? 1 + lcs[i + 1, j + 1] : Math.Max(lcs[i + 1, j], lcs[i, j + 1]);
        }
        int p = 0, q = 0;
        while (p < n || q < m)
        {
            token.ThrowIfCancellationRequested();
            if (p < n && q < m && Same(pre + p, pre + q)) { Equal(pre + p++, pre + q++); continue; }
            int startP = p, startQ = q;
            while (p < n || q < m)
            {
                if (p < n && q < m && Same(pre + p, pre + q)) break;
                if (p < n && (q == m || lcs[p + 1, q] >= lcs[p, q + 1])) p++; else q++;
            }
            int pairs = Math.Min(p - startP, q - startQ);
            for (int k = 0; k < pairs; k++) { int i = pre + startP + k, j = pre + startQ + k; rows.Add(new(i + 1, a[i], j + 1, b[j], "Changed")); changed++; }
            for (int k = startP + pairs; k < p; k++) { int i = pre + k; rows.Add(new(i + 1, a[i], null, "", "Removed")); removed++; }
            for (int k = startQ + pairs; k < q; k++) { int j = pre + k; rows.Add(new(null, "", j + 1, b[j], "Added")); added++; }
        }
        for (int k = suffix; k > 0; k--) Equal(a.Length - k, b.Length - k);
        return new(rows, added, removed, changed);
    }
}
