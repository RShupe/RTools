using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

// Framework 4.7.2 does not provide the newer string and stream overloads.
namespace RTools.Core
{
    internal static class Compat
    {
        public static bool Contains(this string s, string value, StringComparison comparison) => s.IndexOf(value, comparison) >= 0;
        public static bool Contains(this string s, char value) => s.IndexOf(value) >= 0;
        public static bool StartsWith(this string s, char value) => s.Length != 0 && s[0] == value;
        public static bool EndsWith(this string s, char value) => s.Length != 0 && s[s.Length - 1] == value;
        public static string Replace(string s, string from, string to, StringComparison comparison)
        {
            if (from.Length == 0) throw new ArgumentException("Find cannot be empty.");
            var b = new StringBuilder(); int start = 0, index;
            while ((index = s.IndexOf(from, start, comparison)) >= 0) { b.Append(s, start, index - start).Append(to); start = index + from.Length; }
            return b.Append(s, start, s.Length - start).ToString();
        }
        public static int PopCount(uint x) { int count = 0; while (x != 0) { x &= x - 1; count++; } return count; }
        public static bool IsFinite(double d) => !double.IsNaN(d) && !double.IsInfinity(d);
        public static void ReadExactly(Stream stream, byte[] buffer, int count)
        {
            int read = 0; while (read < count) { int n = stream.Read(buffer, read, count - read); if (n == 0) throw new EndOfStreamException("File changed during comparison."); read += n; }
        }
    }
    internal static class WindowsPaths
    {
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)] private static extern SafeFileHandle CreateFile(string name, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)] private static extern uint GetFinalPathNameByHandle(SafeFileHandle handle, StringBuilder path, uint capacity, uint flags);
        [StructLayout(LayoutKind.Sequential)] private struct Info { public uint Attributes, CreationLow, CreationHigh, AccessLow, AccessHigh, WriteLow, WriteHigh, Volume, SizeHigh, SizeLow, Links, IndexHigh, IndexLow; }
        [DllImport("kernel32.dll", SetLastError=true)] private static extern bool GetFileInformationByHandle(SafeFileHandle handle, out Info info);
        private static SafeFileHandle Open(string path) => CreateFile(path, 0, 7, IntPtr.Zero, 3, 0x02000000, IntPtr.Zero);
        public static string Resolve(string path)
        {
            string full = Path.GetFullPath(path);
            if (!File.Exists(full) && !Directory.Exists(full)) { var parent = Path.GetDirectoryName(full); return parent == null ? full : Path.Combine(Resolve(parent), Path.GetFileName(full)); }
            using var handle = Open(full);
            if (handle.IsInvalid) throw new IOException("Cannot verify file identity: " + full);
            var b = new StringBuilder(32768); uint n = GetFinalPathNameByHandle(handle, b, (uint)b.Capacity, 0);
            if (n == 0 || n >= b.Capacity) throw new IOException("Cannot resolve the actual file path: " + full);
            var resolved = b.ToString();
            if (resolved.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase)) return @"\\" + resolved.Substring(8);
            if (resolved.StartsWith(@"\\?\", StringComparison.Ordinal) && resolved.Length > 5 && resolved[5] == ':') return resolved.Substring(4);
            return resolved;
        }
        public static bool SameFile(string a, string b)
        {
            if (!File.Exists(a) || !File.Exists(b)) return false;
            using var x = Open(a); using var y = Open(b);
            if (x.IsInvalid || y.IsInvalid || !GetFileInformationByHandle(x, out var i) || !GetFileInformationByHandle(y, out var j)) throw new IOException("Cannot verify that the input and output are different files.");
            return i.Volume == j.Volume && i.IndexHigh == j.IndexHigh && i.IndexLow == j.IndexLow;
        }
    }
}
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }
