namespace RTools.Core;

public sealed record FolderItem(string Path, long Bytes, long Files, long Folders)
{
    public string Size => FolderAnalyzer.FormatSize(Bytes);
}
public sealed record FolderResult(long Bytes, long Files, long Folders, long Skipped, IReadOnlyList<FolderItem> Children, IReadOnlyList<FolderItem> Largest);
public static class FolderAnalyzer
{
    public static string FormatSize(long bytes)
    {
        double n = bytes; string[] units = ["B", "KiB", "MiB", "GiB", "TiB", "PiB"]; int i = 0;
        while (n >= 1024 && i < units.Length - 1) { n /= 1024; i++; }
        return $"{n:N2} {units[i]}";
    }
    public static FolderResult Scan(string root, int largestCount = 100, CancellationToken token = default, IProgress<string>? progress = null)
    {
        root = Path.GetFullPath(string.IsNullOrWhiteSpace(root) ? throw new ArgumentException("Select a folder.") : root);
        if (!Directory.Exists(root)) throw new DirectoryNotFoundException(root);
        if ((File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0) throw new IOException("Choose a physical folder instead of a symbolic link or junction.");
        if (largestCount is < 1 or > 1000) throw new ArgumentException("Largest-file count must be 1–1000.");
        long total = 0, files = 0, folders = 0, skipped = 0;
        var children = new List<FolderItem>(); var heap = new SortedDictionary<long, Queue<FolderItem>>(); int heapCount = 0;
        IEnumerable<string> Entries(string path)
        {
            // Enumerate lazily; catch an inaccessible directory without stopping the scan.
            IEnumerator<string>? it = null;
            try { it = Directory.EnumerateFileSystemEntries(path).GetEnumerator(); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { skipped++; }
            if (it is null) yield break;
            using (it)
            {
                while (true)
                {
                    bool next = false;
                    try { next = it.MoveNext(); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { skipped++; }
                    if (!next) break;
                    yield return it.Current;
                }
            }
        }
        void AddFile(string path, ref long bytes, ref long count)
        {
            long len = new FileInfo(path).Length; bytes += len; count++; total += len; files++;
            if (heapCount < largestCount || len > heap.First().Key)
            {
                if (!heap.TryGetValue(len, out var bucket)) heap.Add(len, bucket = new Queue<FolderItem>());
                bucket.Enqueue(new FolderItem(path, len, 1, 0)); heapCount++;
                if (heapCount > largestCount) { var smallest = heap.First(); smallest.Value.Dequeue(); if (smallest.Value.Count == 0) heap.Remove(smallest.Key); heapCount--; }
            }
            if (files % 1000 == 0) progress?.Report($"Scanning… {files:N0} files · {FormatSize(total)}");
        }
        foreach (var child in Entries(root))
        {
            token.ThrowIfCancellationRequested(); long bytes = 0, count = 0, dirs = 0;
            try
            {
                var attr = File.GetAttributes(child);
                if ((attr & FileAttributes.ReparsePoint) != 0) { skipped++; continue; }
                if ((attr & FileAttributes.Directory) == 0) AddFile(child, ref bytes, ref count);
                else
                {
                    var stack = new Stack<string>(); stack.Push(child);
                    while (stack.Count > 0)
                    {
                        var dir = stack.Pop();
                        token.ThrowIfCancellationRequested(); dirs++; folders++;
                        foreach (var path in Entries(dir))
                        {
                            token.ThrowIfCancellationRequested();
                            try
                            {
                                var a = File.GetAttributes(path);
                                if ((a & FileAttributes.ReparsePoint) != 0) { skipped++; continue; }
                                if ((a & FileAttributes.Directory) != 0) stack.Push(path); else AddFile(path, ref bytes, ref count);
                            }
                            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { skipped++; }
                        }
                    }
                }
                children.Add(new(child, bytes, count, dirs));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { skipped++; }
        }
        return new(total, files, folders, skipped, children.OrderByDescending(x => x.Bytes).ToArray(), heap.Reverse().SelectMany(x => x.Value).ToArray());
    }
}
