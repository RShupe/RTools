using System.Text;

namespace RTools.Core;

public sealed record CsvData(string[] Headers, List<string[]> Rows);
public static class CsvWorkbench
{
    public const int MaxRows = 25_000, MaxColumns = 200, MaxCells = 250_000;
    public static CsvData Read(string path, char separator, bool hasHeader, CancellationToken token = default)
    {
        path = FileTools.Input(path);
        if (new FileInfo(path).Length > 20_000_000) throw new InvalidDataException("CSV Workbench supports files up to 20 MB.");
        using var reader = FileTools.OpenText(path);
        var rows = new List<string[]>(); var row = new List<string>(); var field = new StringBuilder();
        bool quoted = false, closed = false, started = false; int chars = 0, columns = 0;
        void Field()
        {
            row.Add(field.ToString()); field.Clear(); closed = false; started = false;
            if (row.Count > MaxColumns) throw new InvalidDataException($"Maximum {MaxColumns} columns.");
        }
        void Record()
        {
            Field(); if (columns == 0) columns = row.Count;
            if (row.Count != columns) throw new InvalidDataException($"Record {rows.Count + 1} has {row.Count} columns; expected {columns}.");
            rows.Add(row.ToArray()); row.Clear();
            if (rows.Count > MaxRows + (hasHeader ? 1 : 0) || (long)rows.Count * columns > MaxCells + columns) throw new InvalidDataException("CSV Workbench limit: 25,000 rows and 250,000 data cells.");
        }
        int value;
        while ((value = reader.Read()) != -1)
        {
            if ((++chars & 4095) == 0) token.ThrowIfCancellationRequested();
            if (chars > 20_000_000) throw new InvalidDataException("CSV character limit exceeded.");
            char c = (char)value;
            if (quoted)
            {
                if (c == '"') { if (reader.Peek() == '"') { reader.Read(); field.Append('"'); } else { quoted = false; closed = true; } }
                else field.Append(c);
            }
            else if (c == separator) Field();
            else if (c is '\r' or '\n') { if (c == '\r' && reader.Peek() == '\n') reader.Read(); Record(); }
            else if (closed) throw new InvalidDataException($"Unexpected character after closing quote in record {rows.Count + 1}.");
            else if (c == '"') { if (started) throw new InvalidDataException($"Unexpected quote in record {rows.Count + 1}."); quoted = true; started = true; }
            else { started = true; field.Append(c); }
        }
        token.ThrowIfCancellationRequested();
        if (quoted) throw new InvalidDataException("Unclosed quoted field at end of file.");
        if (started || closed || row.Count > 0 || field.Length > 0) Record();
        if (rows.Count == 0) throw new InvalidDataException("The file is empty.");
        string[] headers = hasHeader ? rows[0] : Enumerable.Range(1, columns).Select(n => $"Column {n}").ToArray();
        if (hasHeader) rows.RemoveAt(0);
        ValidateSize(rows.Count, columns);
        return new(headers, rows);
    }
    public static void ValidateSize(int rows, int columns)
    {
        if (columns < 1 || columns > MaxColumns || rows > MaxRows || (long)rows * columns > MaxCells) throw new InvalidDataException("Limit: 25,000 rows, 200 columns and 250,000 cells.");
    }
    public static void Write(string path, string[] headers, IEnumerable<string[]> rows, char separator, bool includeHeader, bool spreadsheetSafe, CancellationToken token = default)
    {
        path = FileTools.Output(path); var temp = FileTools.TempFor(path);
        try
        {
            using (var writer = FileTools.NewText(temp))
            {
                string Escape(string s)
                {
                    var trimmed = s.TrimStart();
                    if (spreadsheetSafe && (s.StartsWith('\t') || s.StartsWith('\r') || s.StartsWith('\n') || (trimmed.Length > 0 && "=+-@".Contains(trimmed[0])))) s = "'" + s;
                    return s.Contains(separator) || s.IndexOfAny(['"', '\n', '\r']) >= 0 ? '"' + s.Replace("\"", "\"\"") + '"' : s;
                }
                if (includeHeader) writer.WriteLine(string.Join(separator.ToString(), headers.Select(Escape)));
                int count = 0;
                foreach (var row in rows)
                {
                    token.ThrowIfCancellationRequested(); ValidateSize(++count, headers.Length);
                    if (row.Length != headers.Length) throw new InvalidDataException("Inconsistent export columns.");
                    writer.WriteLine(string.Join(separator.ToString(), row.Select(Escape)));
                }
            }
            token.ThrowIfCancellationRequested(); FileTools.Commit(temp, path);
        }
        finally { FileTools.Cleanup(temp); }
    }
}
