using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using RTools.Core;

namespace RTools.UI;

internal static class WindowsSystemInfo
{
    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatus
    {
        public uint Length, Load;
        public ulong TotalPhysical, AvailablePhysical, TotalPageFile, AvailablePageFile, TotalVirtual, AvailableVirtual, AvailableExtendedVirtual;
    }
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatus status);
    [DllImport("kernel32.dll")] private static extern ulong GetTickCount64();

    public static string Collect(CancellationToken token, IProgress<string>? progress = null)
    {
        var b = new StringBuilder();
        void Line(string key, object? value) => b.AppendLine($"{key,-24} {value ?? "Unavailable"}");
        void Heading(string text) { b.AppendLine(); b.AppendLine(text); b.AppendLine(new string('─', 40)); }
        string? ReadRegistry(string key, string value) { try { return Registry.GetValue(key, value, null)?.ToString(); } catch { return null; } }
        token.ThrowIfCancellationRequested(); progress?.Report("Collecting computer information…");
        Heading("COMPUTER"); Line("Machine name", Environment.MachineName); Line("Operating system", ReadRegistry(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName") ?? Environment.OSVersion.VersionString); Line("Architecture", Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit"); Line("Process architecture", Environment.Is64BitProcess ? "64-bit" : "32-bit"); Line("Windows display version", ReadRegistry(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "DisplayVersion")); Line("Windows build", ReadRegistry(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber") ?? Environment.OSVersion.Version.ToString()); Line("Uptime", TimeSpan.FromMilliseconds((double)GetTickCount64()).ToString(@"d\.hh\:mm\:ss")); Line("Local time", DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz")); Line("Time zone", TimeZoneInfo.Local.DisplayName); Line(".NET runtime", ".NET Framework CLR " + Environment.Version);
        Heading("HARDWARE"); Line("Manufacturer", ReadRegistry(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS", "SystemManufacturer")); Line("Model", ReadRegistry(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS", "SystemProductName")); Line("CPU", ReadRegistry(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0", "ProcessorNameString")); Line("Logical processors", Environment.ProcessorCount);
        var memory = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        if (GlobalMemoryStatusEx(ref memory)) { Line("Physical memory", FolderAnalyzer.FormatSize((long)memory.TotalPhysical)); Line("Available memory", FolderAnalyzer.FormatSize((long)memory.AvailablePhysical)); Line("Memory load", memory.Load + "%"); } else Line("Physical memory", "Unavailable");
        Heading("LOCAL STORAGE"); progress?.Report("Collecting storage information…");
        foreach (var drive in DriveInfo.GetDrives())
        {
            token.ThrowIfCancellationRequested();
            // Do not contact disconnected network drives or removable media merely to refresh this view.
            if (drive.DriveType is not (DriveType.Fixed or DriveType.Ram)) { Line(drive.Name, drive.DriveType + " (capacity not queried)"); continue; }
            try { if (drive.IsReady) Line(drive.Name, $"{drive.DriveFormat} · {FolderAnalyzer.FormatSize(drive.AvailableFreeSpace)} free / {FolderAnalyzer.FormatSize(drive.TotalSize)} total"); else Line(drive.Name, "Not ready"); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { Line(drive.Name, "Unavailable"); }
        }
        Heading("NETWORK ADAPTERS"); progress?.Report("Collecting local adapter information…");
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces().OrderBy(x => x.Name))
        {
            token.ThrowIfCancellationRequested();
            try
            {
                Line(nic.Name, $"{nic.NetworkInterfaceType} · {nic.OperationalStatus}");
                if (nic.OperationalStatus != OperationalStatus.Up) continue;
                var p = nic.GetIPProperties(); Line("  Addresses", string.Join(", ", p.UnicastAddresses.Select(x => x.Address.ToString()))); Line("  Gateways", string.Join(", ", p.GatewayAddresses.Select(x => x.Address.ToString()))); Line("  DNS servers", string.Join(", ", p.DnsAddresses.Select(x => x.ToString())));
            }
            catch (NetworkInformationException) { Line("  Details", "Unavailable"); }
        }
        return b.ToString().Trim();
    }
}
