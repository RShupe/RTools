using System.Runtime.InteropServices;
using RTools.Core;

namespace RTools;

internal static class Program
{
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool AttachConsole(int processId);

    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Length == 0 || (args.Length == 1 && args[0] == "--startup-test"))
            return RunDesktop(args.Length != 0);
        AttachConsole(-1);
        try
        {
            switch (args[0].ToLowerInvariant())
            {
                case "help": case "--help": case "-h":
                    Usage(); return 0;
                case "tfj" when args.Length >= 5:
                    if (!int.TryParse(args[2], out int lines) || !new[] { "Y", "N" }.Contains(args[3], StringComparer.OrdinalIgnoreCase)) throw new ArgumentException("Use a blank-line count and Y/N for deletion.");
                    var join = FileTools.Join(args.Skip(4), args[1], lines, args[3].Equals("Y", StringComparison.OrdinalIgnoreCase));
                    Console.WriteLine($"Joined {join.Files} files.");
                    foreach (var failure in join.DeleteFailures) Console.Error.WriteLine(failure);
                    return join.DeleteFailures.Count == 0 ? 0 : 1;
                case "sa" when args.Length >= 4:
                    var search = FileTools.SearchAppend(args[1], args[2], args.Skip(3), false, true);
                    Console.WriteLine($"Moved {search.Matched} lines. Backup: {search.Backup ?? "not needed"}"); return 0;
                case "tfj" or "sa" when args.Length == 1:
                    Usage(); return 0;
                default:
                    Usage(); return 2;
            }
        }
        catch (Exception ex) { Console.Error.WriteLine($"Error: {ex.Message}"); return 1; }
    }
    private static int RunDesktop(bool smokeTest)
    {
        string stage = "initializing Windows Forms";
        // Route UI-thread exceptions to this entry point instead of letting
        // a Release executable disappear without a useful error message.
        try
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            stage = "constructing MainForm";
            using var form = new MainForm();
            stage = "binding main-window controls";
            form.StartRuntime(!smokeTest);
            stage = "loading and showing the main window";
            bool shown = false;
            using var smokeTimer = new System.Windows.Forms.Timer { Interval = 750 };
            form.Shown += (_, _) =>
            {
                shown = true;
                stage = "running the main window";
                if (smokeTest) smokeTimer.Start();
            };
            smokeTimer.Tick += (_, _) =>
            {
                smokeTimer.Stop();
                if (!form.Visible || !form.IsHandleCreated)
                    throw new InvalidOperationException("The main window was not visible during startup testing.");
                form.Close();
            };
            Application.Run(form);
            if (!shown) throw new InvalidOperationException("The message loop ended before the main window was shown.");
            return 0;
        }
        catch (Exception ex)
        {
            string? path = WriteStartupError(stage, ex);
            if (!smokeTest)
            {
                string details = "RTools could not continue while " + stage + ".\n\n" + ex.GetType().Name + ": " + ex.Message;
                if (path != null) details += "\n\nDiagnostic log: " + path;
                try { MessageBox.Show(details, "RTools - Application error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                catch { /* The log is still available if Windows Forms cannot show a dialog. */ }
            }
            return 1;
        }
    }

    internal static string? WriteStartupError(string stage, Exception ex)
    {
        foreach (var directory in new[] {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RTools"),
            Path.Combine(Path.GetTempPath(), "RTools") })
        {
            try
            {
                Directory.CreateDirectory(directory);
                string path = Path.Combine(directory, "startup-error.log");
                File.WriteAllText(path, DateTimeOffset.Now.ToString("o") + Environment.NewLine +
                    "RTools " + MainForm.DisplayVersion + Environment.NewLine +
                    "Stage: " + stage + Environment.NewLine + ex.ToString());
                return path;
            }
            catch { /* Try the fallback location. */ }
        }
        return null;
    }

    private static void Usage()
    {
        Console.WriteLine(ConsoleHelp.Usage(MainForm.DisplayVersion));
    }
}
