using System.Globalization;
using System.Text.RegularExpressions;

namespace RTools.Core;

public sealed record RenameOptions(string Find, string Replace, bool Regex, bool IgnoreCase, string Prefix, string Suffix, bool Number, int Start, int Digits, string Case);
public sealed record RenameItem(string Source, string Destination, string Status)
{
    public string OldName => Path.GetFileName(Source);
    public string NewName => Path.GetFileName(Destination);
}
public static class BulkRename
{
    public static IReadOnlyList<RenameItem> Preview(IEnumerable<string> files, RenameOptions options, CancellationToken token = default)
    {
        var paths = files.Select(FileTools.Input).Distinct(FileTools.PathComparer).OrderBy(p => p, FileTools.PathComparer).ToArray();
        if (paths.Length == 0 || paths.Length > 10_000) throw new ArgumentException("Select between 1 and 10,000 files.");
        if (options.Start < 0 || options.Digits is < 1 or > 9) throw new ArgumentException("Invalid numbering settings.");
        Regex? regex = options.Regex && options.Find.Length > 0 ? new Regex(options.Find, options.IgnoreCase ? RegexOptions.IgnoreCase | RegexOptions.CultureInvariant : RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(200)) : null;
        var result = new List<RenameItem>();
        for (int i = 0; i < paths.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            var p = paths[i]; var stem = Path.GetFileNameWithoutExtension(p); var ext = Path.GetExtension(p);
            if (options.Find.Length > 0) stem = regex is not null ? regex.Replace(stem, options.Replace) : Compat.Replace(stem, options.Find, options.Replace, options.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            stem = options.Case switch { "Lowercase" => stem.ToLowerInvariant(), "Uppercase" => stem.ToUpperInvariant(), _ => stem };
            string name = options.Prefix + (options.Number ? ((long)options.Start + i).ToString("D" + options.Digits, CultureInfo.InvariantCulture) + "_" : "") + stem + options.Suffix + ext;
            string status = ValidName(name) ? "Ready" : "Invalid filename";
            string dest = status == "Ready" ? Path.Combine(Path.GetDirectoryName(p)!, name) : p;
            if (status == "Ready" && string.Equals(p, dest, StringComparison.Ordinal)) status = "Unchanged";
            else if (status == "Ready" && !CaseOnly(p, dest) && (File.Exists(dest) || Directory.Exists(dest))) status = "Destination already exists";
            if ((File.GetAttributes(p) & FileAttributes.ReparsePoint) != 0) status = "Symbolic link skipped";
            result.Add(new(p, dest, status));
        }
        var duplicates = new HashSet<string>(result.Where(r => r.Status == "Ready").GroupBy(r => r.Destination, FileTools.PathComparer).Where(g => g.Count() > 1).Select(g => g.Key), FileTools.PathComparer);
        return result.Select(r => r.Status == "Ready" && duplicates.Contains(r.Destination) ? r with { Status = "Duplicate destination" } : r).ToArray();
    }
    private static bool ValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 255 || name.EndsWith('.') || name.EndsWith(' ') || name.Any(c => c < 32 || "<>:\"/\\|?*".Contains(c))) return false;
        var device = name.Split('.')[0].TrimEnd().ToUpperInvariant();
        return !Regex.IsMatch(device, @"^(CON|PRN|AUX|NUL|COM[1-9¹²³]|LPT[1-9¹²³]|CONIN\$|CONOUT\$)$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    }
    private static bool CaseOnly(string source, string destination) => FileTools.PathComparer.Equals(source, destination) && !string.Equals(source, destination, StringComparison.Ordinal);
    private static void MoveName(string source, string destination)
    {
        if (!CaseOnly(source, destination)) { File.Move(source, destination); return; }
        // Two-step rename also works on case-insensitive Windows volumes. Never overwrite.
        var temp = FileTools.TempFor(source);
        File.Move(source, temp);
        try { File.Move(temp, destination); }
        catch (Exception ex)
        {
            try { File.Move(temp, source); }
            catch (Exception rollback) { throw new IOException($"Case-only rename failed and could not restore {source}. Recover the file at {temp}. {rollback.Message}", ex); }
            throw;
        }
    }
    // The exact reviewed plan is revalidated; no destinations are ever overwritten.
    public static IReadOnlyList<RenameItem> Apply(IReadOnlyList<RenameItem> plan, CancellationToken token = default)
    {
        if (plan.Any(x => x.Status is not ("Ready" or "Unchanged"))) throw new InvalidOperationException("Resolve every preview conflict before applying.");
        var batch = plan.Where(x => x.Status == "Ready").ToArray();
        if (batch.Length == 0) throw new InvalidOperationException("No changes to apply.");
        var targets = new HashSet<string>(FileTools.PathComparer);
        var sources = new HashSet<string>(FileTools.PathComparer);
        foreach (var x in batch)
        {
            token.ThrowIfCancellationRequested();
            var source = FileTools.Input(x.Source); var dest = Path.GetFullPath(x.Destination);
            if (!sources.Add(source) || !targets.Add(dest) || !FileTools.PathComparer.Equals(Path.GetDirectoryName(source), Path.GetDirectoryName(dest)) || !ValidName(Path.GetFileName(dest)) || string.Equals(source, dest, StringComparison.Ordinal) || (!CaseOnly(source, dest) && (File.Exists(dest) || Directory.Exists(dest))) || (File.GetAttributes(source) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("The reviewed plan is no longer valid: " + dest);
        }
        token.ThrowIfCancellationRequested();
        // Once commit starts, finish or try to roll back, even if cancellation is requested.
        var completed = new List<RenameItem>();
        try { foreach (var x in batch) { MoveName(x.Source, x.Destination); completed.Add(x); } }
        catch (Exception ex)
        {
            var failures = new List<string>();
            foreach (var x in completed.AsEnumerable().Reverse()) try { MoveName(x.Destination, x.Source); } catch (Exception rollback) { failures.Add(x.Destination + " -> " + x.Source + ": " + rollback.Message); }
            throw new IOException("Rename failed: " + ex.Message + (failures.Count == 0 ? "\nCompleted renames were rolled back." : "\nRollback incomplete; review these paths:\n" + string.Join("\n", failures)), ex);
        }
        return completed.Select(x => new RenameItem(x.Destination, x.Source, "Ready")).ToArray();
    }
}
