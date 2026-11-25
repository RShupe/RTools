using RTools.Core;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

// Dependency-free regression runner. `RTools.Tests.exe` returns nonzero on failure.
var root = Path.Combine(Path.GetTempPath(), "rtools-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
int passed = 0, failed = 0;
void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
void Test(string name, Action action)
{
    try { action(); Console.WriteLine("PASS  " + name); passed++; }
    catch (Exception ex) { Console.Error.WriteLine($"FAIL  {name}: {ex.Message}"); failed++; }
}
string FileAt(string name, string text)
{
    var path = Path.Combine(root, name); File.WriteAllText(path, text, new UTF8Encoding(false)); return path;
}
void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { return; } throw new Exception($"Expected {typeof(T).Name}.");
}
string Record(string prefix, string code, string company = "ACME, LLC")
{
    // Fictional test data only; no production cardholders or account numbers.
    var row = Enumerable.Repeat(' ', 440).ToArray();
    void Put(int offset, string text) => text.CopyTo(0, row, offset, text.Length);
    Put(0, prefix + "0000000001"); Put(23, "202601011200"); Put(74, code); Put(91, "12.34"); Put(145, "0000001234"); Put(207, "TEST CITY"); Put(224, company); Put(314, "ACTIVE"); Put(392, "FAKE-MERCHANT"); Put(415, "1234"); return new string(row);
}
try
{
    Test("Join preserves order and replaces an existing output", () =>
    {
        var a = FileAt("a.txt", "alpha"); var b = FileAt("b.txt", "beta"); var output = FileAt("joined.txt", "old");
        var result = FileTools.Join([b, a], output, 1, false);
        Check(result.Files == 2 && File.ReadAllText(output) == "beta" + Environment.NewLine + Environment.NewLine + "alpha", "Unexpected output.");
        Check(File.Exists(a) && File.Exists(b), "Input was deleted.");
    });
    Test("Join refuses identical input and output", () =>
    {
        var a = FileAt("same.txt", "keep"); Throws<ArgumentException>(() => FileTools.Join([a], a, 1, true)); Check(File.ReadAllText(a) == "keep", "Source changed.");
    });
    Test("Cancelled join preserves output and originals", () =>
    {
        var a = FileAt("cancel.txt", "keep"); var b = FileAt("cancel-out.txt", "old"); using var c = new CancellationTokenSource(); c.Cancel();
        Throws<OperationCanceledException>(() => FileTools.Join([a], b, 1, true, c.Token)); Check(File.Exists(a) && File.ReadAllText(b) == "old", "Files changed on cancellation.");
    });
    Test("Join invalid output never deletes originals", () =>
    {
        var a = FileAt("bad-out.txt", "keep"); Throws<DirectoryNotFoundException>(() => FileTools.Join([a], Path.Combine(root, "missing", "x.txt"), 1, true)); Check(File.Exists(a), "Source was deleted.");
    });
    Test("Join deletes only after success", () =>
    {
        var a = FileAt("delete.txt", "content"); var b = Path.Combine(root, "deleted-out.txt"); var result = FileTools.Join([a], b, 0, true);
        Check(!File.Exists(a) && File.ReadAllText(b) == "content" && result.DeleteFailures.Count == 0, "Unexpected deletion result.");
    });
    Test("Search preview and copy preserve source", () =>
    {
        var a = FileAt("search.txt", "error A\nOK\nERROR B\n"); var b = FileAt("search-out.txt", "existing");
        var preview = FileTools.PreviewSearch(a, ["error", "error"], true); Check(preview.Matched == 2 && preview.Unmatched == 1, "Wrong preview counts.");
        var result = FileTools.SearchAppend(a, b, ["error"], true, false);
        Check(result.Matched == 2 && result.Backup is null && File.ReadAllText(a) == "error A\nOK\nERROR B\n", "Source changed.");
        Check(File.ReadAllLines(b).SequenceEqual(["existing", "error A", "ERROR B"]), "Append result differs.");
    });
    Test("Search move preserves an exact backup", () =>
    {
        var a = FileAt("move.txt", "match\r\nkeep\r\nmatch again"); var original = File.ReadAllBytes(a); var b = Path.Combine(root, "move-out.txt");
        var result = FileTools.SearchAppend(a, b, ["match"], false, true);
        Check(result.Matched == 2 && result.Backup is not null && File.ReadAllBytes(result.Backup).SequenceEqual(original), "Backup missing or differs.");
        Check(File.ReadAllLines(a).SequenceEqual(["keep"]) && File.ReadAllLines(b).SequenceEqual(["match", "match again"]), "Incorrect moved lines.");
    });
    Test("Search no matches leaves both files unchanged", () =>
    {
        var a = FileAt("nomatch.txt", "a"); var b = FileAt("nomatch-out.txt", "b"); var result = FileTools.SearchAppend(a, b, ["zzz"], true, true);
        Check(result.Matched == 0 && result.Backup is null && File.ReadAllText(a) == "a" && File.ReadAllText(b) == "b", "Files changed.");
    });
    Test("Search rejects identical source and destination", () =>
    {
        var a = FileAt("search-same.txt", "a"); Throws<ArgumentException>(() => FileTools.SearchAppend(a, a, ["a"], true, true));
    });
    Test("Report preserves final record and masks identifiers", () =>
    {
        var data = string.Join("\r\n", Enumerable.Repeat("HEADER", 11)) + "\r\n" + Record("485582", "POS-CHK") + "\r\n" + Record("424447", "POS-CHK") + "\r\n" + Record("485582", "WDL-CHK") + "\r\n" + Record("485582", "POS-CHK", "ABC \"SHOP\"");
        var input = FileAt("report.txt", data); var output = Path.Combine(root, "report.csv");
        var options = new ReportOptions(); var preview = ReportExtractor.Preview(input, options); var result = ReportExtractor.Export(input, output, options);
        Check(preview == result && result.Exportable == 2 && result.OtherCards == 1 && result.Ignored == 1 && result.Malformed == 0, "Incorrect report counts.");
        var lines = File.ReadAllLines(output); Check(lines.Length == 3 && lines[1].Contains("************0001") && lines[1].Contains("******1234"), "Masking or row count failed.");
        Check(lines[1].Contains("\"ACME, LLC\"") && lines[2].Contains("\"ABC \"\"SHOP\"\"\""), "CSV quoting failed.");
    });
    Test("Report with no valid rows does not replace existing output", () =>
    {
        var input = FileAt("invalid-report.txt", "bad report"); var output = FileAt("keep.csv", "old");
        Throws<InvalidDataException>(() => ReportExtractor.Export(input, output, new ReportOptions())); Check(File.ReadAllText(output) == "old", "Existing CSV was replaced.");
    });
    Test("Report skips malformed records and neutralizes spreadsheet formula text", () =>
    {
        var input = FileAt("formula-report.txt", "4855820000000001\r\n" + Record("485582", "POS-CHK", "=1+1")); var output = Path.Combine(root, "formula.csv");
        var result = ReportExtractor.Export(input, output, new ReportOptions(HeaderLines: 0)); Check(result.Malformed == 1 && result.Exportable == 1, "Wrong report counts.");
        Check(File.ReadAllText(output).Contains("\"'=1+1\""), "Formula text not neutralized.");
    });
    Test("Text diff aligns insertions, removals and changes", () =>
    {
        var r = Comparison.CompareLines(["first", "same", "old", "end"], ["first", "new line", "same", "new", "end"]);
        Check(r.Added == 1 && r.Changed == 1 && r.Removed == 0, "Unexpected alignment.");
        Check(r.Rows.Any(x => x.Kind == "Changed" && x.Left == "old" && x.Right == "new"), "Change was not paired.");
        var d = Comparison.CompareLines(["A b", "remove"], ["ab"], true, true);
        Check(d.Removed == 1 && d.Rows[0].Kind == "Equal", "Options or deletion failed.");
    });
    Test("Binary comparison reports first byte offset and length difference", () =>
    {
        var a = FileAt("bin1.txt", "abcdef"); var b = FileAt("bin2.txt", "abcXef");
        Check(Comparison.Binary(a, b).FirstDifference == 3, "Wrong byte offset.");
        File.WriteAllText(b, "abcdefX"); Check(Comparison.Binary(a, b).FirstDifference == 6, "Length difference missed.");
        Check(Comparison.Binary(a, a).FirstDifference is null, "Identical file marked different.");
    });
    Test("Bulk rename previews, applies and undoes without changing contents", () =>
    {
        var a = FileAt("rename-a.txt", "one"); var b = FileAt("rename-b.txt", "two");
        var options = new RenameOptions("rename-", "", false, false, "photo_", "", true, 7, 3, "Uppercase");
        var plan = BulkRename.Preview([a, b], options);
        Check(plan.All(r => r.Status == "Ready") && plan[0].NewName == "photo_007_A.txt" && File.Exists(a), "Incorrect preview or changed files.");
        var undo = BulkRename.Apply(plan); Check(!File.Exists(a) && File.ReadAllText(plan[0].Destination) == "one", "Apply failed.");
        BulkRename.Apply(undo); Check(File.ReadAllText(a) == "one" && File.ReadAllText(b) == "two", "Undo failed.");
    });
    Test("Bulk rename supports case-only filename changes", () =>
    {
        var source = FileAt("rename-case.txt", "keep");
        var options = new RenameOptions("", "", false, false, "", "", false, 1, 3, "Uppercase");
        var plan = BulkRename.Preview([source], options); Check(plan[0].Status == "Ready", "Case-only preview blocked.");
        var undo = BulkRename.Apply(plan);
        Check(Directory.EnumerateFiles(root).Any(p => Path.GetFileName(p) == "RENAME-CASE.txt") && File.ReadAllText(plan[0].Destination) == "keep", "Case-only rename failed.");
        BulkRename.Apply(undo); Check(Directory.EnumerateFiles(root).Any(p => Path.GetFileName(p) == "rename-case.txt"), "Case-only undo failed.");
    });
    Test("Bulk rename refuses existing and duplicate destinations", () =>
    {
        var a = FileAt("collision-a.txt", "a"); var b = FileAt("collision-b.txt", "b"); var c = FileAt("target.txt", "keep");
        var options = new RenameOptions(".+", "target", true, false, "", "", false, 1, 3, "Preserve case");
        var blocked = BulkRename.Preview([a, b], options); Check(blocked.All(r => r.Status == "Destination already exists"), "Existing destination missed.");
        Throws<InvalidOperationException>(() => BulkRename.Apply(blocked)); Check(File.ReadAllText(c) == "keep", "Destination changed.");
        File.Delete(c); var duplicates = BulkRename.Preview([a, b], options); Check(duplicates.All(r => r.Status == "Duplicate destination"), "Duplicate missed.");
        var reserved = BulkRename.Preview([a], options with { Replace = "CON" }); Check(reserved[0].Status == "Invalid filename", "Windows device name accepted.");
        var good = BulkRename.Preview([a], options with { Replace = "unique" }); FileAt("unique.txt", "new");
        Throws<IOException>(() => BulkRename.Apply(good)); Check(File.ReadAllText(a) == "a", "Source changed after stale preview.");
    });
    Test("CSV reads quoted multiline fields and preserves leading zeros", () =>
    {
        var a = FileAt("quoted.csv", "id,description\r\n001,\"hello, \"\"world\"\"\r\nnext\"\r\n002,\r\n");
        var data = CsvWorkbench.Read(a, ',', true); Check(data.Rows.Count == 2 && data.Rows[0][0] == "001" && data.Rows[0][1] == "hello, \"world\"\r\nnext" && data.Rows[1][1] == "", "CSV parsing failed.");
        var b = Path.Combine(root, "roundtrip.tsv"); CsvWorkbench.Write(b, data.Headers, data.Rows, '\t', true, false);
        var round = CsvWorkbench.Read(b, '\t', true); Check(round.Rows.Count == data.Rows.Count && round.Rows.Zip(data.Rows, (aRow, bRow) => aRow.SequenceEqual(bRow)).All(x => x), "Round trip failed.");
    });
    Test("CSV rejects malformed input and preserves existing output on cancellation", () =>
    {
        var bad = FileAt("ragged.csv", "a,b\n1\n"); Throws<InvalidDataException>(() => CsvWorkbench.Read(bad, ',', true));
        var quoted = FileAt("unclosed.csv", "a,b\n1,\"bad"); Throws<InvalidDataException>(() => CsvWorkbench.Read(quoted, ',', true));
        var outPath = FileAt("csv-cancel.csv", "keep"); using var cts = new CancellationTokenSource(); cts.Cancel();
        Throws<OperationCanceledException>(() => CsvWorkbench.Write(outPath, ["a"], [["b"]], ',', true, true, cts.Token)); Check(File.ReadAllText(outPath) == "keep", "Cancelled export overwrote output.");
        CsvWorkbench.Write(outPath, ["a"], [["=1+1"]], ',', true, true); Check(CsvWorkbench.Read(outPath, ',', true).Rows[0][0] == "'=1+1", "Formula guard failed.");
    });
    Test("IPv4 subnet normal, /0, /31 and /32", () =>
    {
        var s = SubnetCalculator.Calculate("192.168.1.130/25", "24");
        Check(s.Network == "192.168.1.128" && s.Broadcast == "192.168.1.255" && s.FirstHost == "192.168.1.129" && s.LastHost == "192.168.1.254" && s.Usable == 126 && s.Mask == "255.255.255.128", "Incorrect /25.");
        Check(SubnetCalculator.Calculate("1.2.3.4", "0.0.0.0").Total == 4294967296UL, "Incorrect /0 size.");
        var pair = SubnetCalculator.Calculate("10.0.0.1", "31"); Check(pair.Usable == 2 && pair.FirstHost == "10.0.0.0" && pair.LastHost == "10.0.0.1", "Incorrect /31.");
        var host = SubnetCalculator.Calculate("255.255.255.255", "32"); Check(host.Usable == 1 && host.FirstHost == host.LastHost, "Incorrect /32.");
        Throws<ArgumentException>(() => SubnetCalculator.Calculate("10.0.0.1", "255.0.255.0")); Throws<ArgumentException>(() => SubnetCalculator.Calculate("10.0.0.256", "24"));
    });
    Test("Log viewer filters severity and retains latest matching entries", () =>
    {
        var path = FileAt("test.log", "INFO started\nERROR old\nWARN disk\nERROR latest\n");
        var log = LogReader.Read(path, "", "Error", 1, true); Check(log.Lines.Count == 1 && log.Lines[0].Text == "ERROR latest", "Incorrect tail.");
        Check(LogReader.Read(path, "DISK", "All", 10, true).Lines[0].Severity == "Warning", "Keyword/severity failed.");
        var longPath = FileAt("long.log", new string('x', 1024 * 1024) + "\nERROR tail\n");
        var tail = LogReader.Read(longPath, "tail", "Error", 10, true); Check(tail.Partial && tail.Lines.Count == 1, "Bounded tail failed.");
    });
    Test("Regex groups and replacement preview", () =>
    {
        var r = RegexWorkbench.Run("order 123, order 456", @"order (?<id>\d+)", "ID=${id}", System.Text.RegularExpressions.RegexOptions.None, true);
        Check(r.Hits.Count == 2 && r.Hits[0].Groups.Contains("id: 123") && r.Replacement == "ID=123, ID=456", "Regex output failed.");
        var limited = RegexWorkbench.Run(new string('a', 1100), "a", "b", System.Text.RegularExpressions.RegexOptions.None, true); Check(limited.Truncated && limited.Replacement is null, "Truncation guard failed.");
    });
    Test("Color HEX and HSL conversion", () =>
    {
        Check(Rgb.Parse("#f35").Hex == "#FF3355", "Short hex conversion failed.");
        var c = Rgb.FromHsl(120, 100, 50); Check(c.Hex == "#00FF00" && Math.Abs(c.Hsl.H - 120) < .01, "HSL conversion failed.");
        Check(Rgb.FromHsl(0, 0, 50).Hex == "#808080", "Gray conversion failed.");
        Throws<ArgumentException>(() => Rgb.Parse("#ZZZZZZ"));
    });
    Test("Updater accepts trimmed version text and detects strictly newer versions", () =>
    {
        var parsed = RTools.UpdateChecker.Parse("\uFEFF v2.2.1.0\r\n");
        Check(parsed == new Version(2,2,1,0), "Version file parsing failed.");
        Check(RTools.UpdateChecker.IsNewer(parsed,new Version(2,2,0,0)), "New version missed.");
        Check(!RTools.UpdateChecker.IsNewer(new Version(2,2),new Version(2,2,0,0)), "Equivalent versions must not prompt.");
        Check(!RTools.UpdateChecker.IsNewer(new Version(1,2,1,2),new Version(2,2,0,0)), "Older remote version must not prompt.");
        Throws<InvalidDataException>(() => RTools.UpdateChecker.Parse("<html>not a version</html>"));
    });
    Test("Main window constructs stock designer controls and full-card buttons on an STA thread", () =>
    {
        // Do not show the form: this exercises InitializeComponent without theme I/O,
        // tool execution or a GitHub update check. It is not a Visual Studio designer test.
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = new RTools.MainForm();
                var suffixes = new[] { "Join", "Search", "Report", "Compare", "Rename", "Csv", "Subnet", "Log", "SystemInfo", "Folder", "Regex", "Color" };
                foreach (var suffix in suffixes)
                {
                    var nav = form.Controls.Find("nav" + suffix, true).Single();
                    Check(nav.GetType() == typeof(Button), "Sidebar must use a stock Button: " + suffix);
                    var tile = form.Controls.Find("home" + suffix, true).Single();
                    Check(tile.GetType() == typeof(Button), "Dashboard must use a single stock Button: " + suffix);
                    Check(tile.Parent.Name == "homeGrid" && tile.Dock == DockStyle.Fill, "Tile must fill its entire grid cell: " + suffix);
                    Check(tile.Controls.Count == 0 && tile.Text.Contains("\n"), "Title and description must be inside the same Button: " + suffix);
                }
                Check(form.Controls.Find("aboutButton", true).Single().Text == "About & Help", "Missing help button.");
                Check(form.Controls.Find("homePage", true).Length == 1, "Missing home page.");
                var homeGrid = (TableLayoutPanel)form.Controls.Find("homeGrid", true).Single();
                Check(form.Controls.Find("homeVersionLabel", true).Length == 0, "Redundant homepage version label must be removed.");
                var firstHeader = form.Controls.Find("homeFilesHeader", true).Single();
                Check(homeGrid.GetRow(firstHeader) == 0 && homeGrid.RowCount == 9 && homeGrid.RowStyles.Count == 9, "First tool group must start at row zero without a version spacer.");
                Check(form.Controls.Find("pageSubtitle", true).Single().Text == "", "Homepage subtitle must be empty at design time.");
                Check(form.Controls.Find("versionLabel", true).Single().Text == "v2.0.0", "Footer version must be retained.");
                Check(form.Controls.Find("homeBanner", true).Length == 0, "Welcome card must be removed.");
                Check(form.Controls.Find("updater", true).Length == 1, "Missing update control.");
                var sidebar = (TableLayoutPanel)form.Controls.Find("side", true).Single();
                var separator = form.Controls.Find("navSeparator", true).Single();
                var settings = form.Controls.Find("settings", true).Single();
                Check(separator.GetType() == typeof(Panel) && sidebar.GetRow(separator) == 3 && sidebar.GetRow(settings) == 4, "Settings divider must be a stock designer control in its own row.");
                Check(sidebar.RowStyles[3].SizeType == SizeType.Absolute && sidebar.RowStyles[3].Height >= 24F, "Settings gap must retain its own height.");
            }
            catch (Exception ex) { error = ex; }
        });
        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Check(thread.Join(TimeSpan.FromSeconds(15)), "Main form construction timed out.");
        if (error != null) throw new Exception("Main form construction failed.", error);
    });
    Test("All thirteen page designers construct stock controls without starting runtime services", () =>
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                var types = typeof(RTools.MainForm).Assembly.GetTypes()
                    .Where(t => t.Namespace == "RTools.UI.Pages" && typeof(UserControl).IsAssignableFrom(t) && t.IsPublic).ToArray();
                Check(types.Length == 13, "Expected twelve tools and About & Help.");
                IEnumerable<Control> Descendants(Control c) => c.Controls.Cast<Control>().SelectMany(child => new[] { child }.Concat(Descendants(child)));
                foreach (var type in types)
                {
                    using var page = (UserControl)Activator.CreateInstance(type)!;
                    Check(page.Controls.Count == 1 && page.Controls[0] is TableLayoutPanel, type.Name + ": missing designer layout.");
                    var controls = Descendants(page).ToArray();
                    Check(controls.Length > 3 && controls.All(c => c.GetType().Assembly == typeof(Control).Assembly), type.Name + ": runtime-only control in the designer.");
                    Check(controls.OfType<Button>().Any(), type.Name + ": missing designer buttons.");
                    if (type.Name == "AboutHelpPage")
                    {
                        var logo = (PictureBox)controls.Single(c => c.Name == "polyvoxLogo");
                        var prefix = controls.Single(c => c.Name == "developedByLabel");
                        var credit = controls.Single(c => c.Name == "polyvoxCredit");
                        Check(prefix.Text == "Developed by" && credit.Text == "Polyvox Softworks", "Missing developer credit.");
                        Check(logo.Image != null && logo.SizeMode == PictureBoxSizeMode.Zoom, "Missing embedded Polyvox logo.");
                        Check(prefix.Parent == logo.Parent && logo.Parent == credit.Parent && prefix.Right <= logo.Left && logo.Right <= credit.Left, "Logo must sit between the two developer credit labels.");
                    }
                }
            }
            catch (Exception ex) { error = ex; }
        });
        thread.IsBackground = true; thread.SetApartmentState(ApartmentState.STA); thread.Start();
        Check(thread.Join(TimeSpan.FromSeconds(60)), "Page designer construction timed out.");
        if (error != null) throw new Exception("Page designer construction failed: " + error.Message, error);
    });
    Test("Designer edits carry through to themed page controls and layout order", () =>
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var view = new RTools.UI.Pages.TextFileJoinerPage();
                var table = (TableLayoutPanel)view.Controls[0];
                var first = table.GetControlFromPosition(0, 0)!;
                var last = table.GetControlFromPosition(0, 2)!;
                string firstName = first.Name, lastName = last.Name;
                table.SuspendLayout(); table.SetRow(first, 2); table.SetRow(last, 0); table.ResumeLayout(true);
                var button = (Button)view.Controls.Find("joinFilesButton", true).Single();
                button.Text = "Custom join action";
                int clicks = 0; button.Click += (_, _) => clicks++;
                var value = (NumericUpDown)view.Controls.Find("spaces", true).Single(); value.Value = 4;
                var summary = (Label)view.Controls.Find("summary", true).Single(); summary.Text = "Custom designer note";
                var ui = RTools.UI.DesignerPageRuntime.Attach(view);
                using var form = new Form { ClientSize = new Size(840, 600) };
                form.Controls.Add(new RTools.UI.ModernScrollHost(ui.Page)); form.Show(); Application.DoEvents();
                Check(ReferenceEquals(ui.Get<Button>("joinFilesButton"), button) && button.Text == "Custom join action", "Designer button edit was discarded.");
                button.PerformClick(); Check(clicks == 1, "Designer button click handler was discarded.");
                Check(ui.Get<RTools.UI.ModernNumber>("spaces").Value == 4, "Designer numeric default was discarded.");
                Check(ui.Get<Label>("summary").Text == "Custom designer note", "Designer label edit was discarded.");
                var layout = (RTools.UI.ContentGroup)view.Controls[0];
                Check(layout.Controls[0].Name == lastName && layout.Controls[layout.Controls.Count - 1].Name == firstName, "Designer row order was discarded.");
                form.Close();
            }
            catch (Exception ex) { error = ex; }
        });
        thread.IsBackground = true; thread.SetApartmentState(ApartmentState.STA); thread.Start();
        Check(thread.Join(TimeSpan.FromSeconds(30)), "Designer edit test timed out.");
        if (error != null) throw new Exception("Designer edit test failed: " + error.Message, error);
    });
    Test("Main window runtime routes full dashboard buttons and About & Help", () =>
    {
        // Exercise the actual click handlers on Windows. Disable the automatic
        // GitHub check; never open a browser or perform file operations in this test.
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = new RTools.MainForm();
                form.StartRuntime(false);
                form.Show();
                Application.DoEvents();
                var suffixes = new[] { "Join", "Search", "Report", "Compare", "Rename", "Csv", "Subnet", "Log", "SystemInfo", "Folder", "Regex", "Color" };
                var home = (Button)form.Controls.Find("navHome", true).Single();
                foreach (var suffix in suffixes)
                {
                    home.PerformClick();
                    Check(form.ActiveTool == "Home", "Home navigation failed.");
                    var subtitle = form.Controls.Find("pageSubtitle", true).Single();
                    var workspace = (TableLayoutPanel)form.Controls.Find("workspace", true).Single();
                    Check(!subtitle.Visible && subtitle.Text == "", "Homepage subtitle must be hidden.");
                    float homeHeaderHeight = workspace.RowStyles[0].Height;
                    var tile = (Button)form.Controls.Find("home" + suffix, true).Single();
                    Check(tile.CanSelect, "Dashboard tile cannot be activated: " + suffix);
                    tile.PerformClick();
                    Check(form.ActiveTool == tile.AccessibleName, "Dashboard navigation failed: " + suffix);
                    Check(subtitle.Visible && subtitle.Text.Length > 0 && workspace.RowStyles[0].Height > homeHeaderHeight, "Tool page subtitle and normal header height must be restored.");
                }
                ((Button)form.Controls.Find("aboutButton", true).Single()).PerformClick();
                Check(form.ActiveTool == "About & Help", "Help navigation failed.");
                var prefix = form.Controls.Find("developedByLabel", true).Single();
                var credit = form.Controls.Find("polyvoxCredit", true).Single();
                Check(prefix.Text == "Developed by" && credit.Text == "Polyvox Softworks", "Missing developer credit.");
                var logo = (PictureBox)form.Controls.Find("polyvoxLogo", true).Single();
                Check(logo.Image != null && prefix.Parent == logo.Parent && logo.Parent == credit.Parent && prefix.Right <= logo.Left && logo.Right <= credit.Left, "Embedded developer logo missing or misplaced.");
                Check(form.Controls.Find("polyvoxFavicon", true).Length == 0, "A separate Polyvox branding area remains.");
                Check(form.Controls.Find("nav", true).Single().Parent is RTools.UI.ModernScrollHost, "Navigation scrollbar was not attached.");
                Check(form.Controls.Find("homePage", true).Single().Parent is RTools.UI.ModernScrollHost, "Home scrollbar was not attached.");
                IEnumerable<Control> Descendants(Control c) => c.Controls.Cast<Control>().SelectMany(child => new[] { child }.Concat(Descendants(child)));
                var controls = Descendants(form).ToArray();
                var scrollable = controls.Where(c => c is TextBox text && text.Multiline || c is RichTextBox || c is ListBox || c is DataGridView).ToArray();
                Check(scrollable.Length >= 12 && scrollable.All(c => c.Parent is RTools.UI.ModernScrollHost), "A tool editor, file list, or grid lacks its modern scrollbar host.");
                Check(!controls.Any(c => c is ComboBox) && controls.OfType<RTools.UI.ModernChoice>().Count() >= 6, "A tool still has a native combo box.");
                Check(!controls.Any(c => c is TrackBar || c is NumericUpDown), "A tool still has a stock slider or numeric spinner.");
                Check(controls.OfType<RTools.UI.ModernSlider>().Count() == 3 && controls.OfType<RTools.UI.ModernNumber>().Count() >= 8, "Missing themed slider or numeric spinner.");
                Check(controls.OfType<CheckBox>().All(c => c is RTools.UI.ModernCheckBox), "A tool still has a stock checkbox.");
                Check(controls.OfType<CheckedListBox>().All(c => c is RTools.UI.ModernCheckedListBox), "A checked list still uses stock rendering.");
                Check(controls.OfType<TextBox>().Where(c => !c.Multiline).All(c => c.Parent is RTools.UI.ModernInputFrame || c.Parent is RTools.UI.ModernNumber), "A single-line editor is missing its themed frame.");
                form.Close();
            }
            catch (Exception ex) { error = ex; }
        });
        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Check(thread.Join(TimeSpan.FromSeconds(45)), "Runtime navigation smoke test timed out.");
        if (error != null) throw new Exception("Runtime navigation failed.", error);
    });
    Test("Every page stops at its actual final control plus fixed bottom padding", () =>
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = new RTools.MainForm();
                form.StartRuntime(false);
                form.Show(); Application.DoEvents();
                form.ClientSize = new Size(950, 650); Application.DoEvents();
                var container = form.Controls.Find("content", true).Single();
                Panel CurrentPage() => (Panel)container.Controls.OfType<RTools.UI.ModernScrollHost>().Single(c => c.Visible).Controls.OfType<Panel>().First(c => c.AutoScroll);
                void AssertBounded(string name)
                {
                    Application.DoEvents();
                    var page = CurrentPage();
                    Check(RTools.UI.PageContentLayout.TryGet(page, out var layout), name + ": missing page layout.");
                    layout.Refresh(); Application.DoEvents();
                    var body = page.Controls.Cast<Control>().Single();
                    var host = (RTools.UI.ModernScrollHost)page.Parent;
                    Check(page.AutoScrollMargin.IsEmpty && page.Padding.Bottom == 0, name + ": trailing scroll margin.");
                    int expected = Math.Max(0, body.Bottom - page.AutoScrollPosition.Y + RTools.UI.PageContentLayout.BottomPadding - page.ClientSize.Height);
                    Check(layout.Maximum == expected, name + ": measured height differs from actual content.");
                    Check(!page.HorizontalScroll.Visible, name + ": unexpected horizontal page overflow.");
                    if (expected > 0)
                    {
                        Check(Math.Max(0, page.VerticalScroll.Maximum - page.VerticalScroll.LargeChange + 1) == expected, name + ": native range exceeds content.");
                        for (int i = 0; i < 3; i++)
                        {
                            host.ScrollTo(false, int.MaxValue);
                            Application.DoEvents();
                            Check(-page.AutoScrollPosition.Y == expected, name + ": rail did not stop at the end.");
                            Check(page.ClientSize.Height - body.Bottom == RTools.UI.PageContentLayout.BottomPadding, name + ": unexpected space after last control.");
                        }
                    }
                    else Check(!page.VerticalScroll.Visible && page.AutoScrollPosition.Y == 0, name + ": scrolling on a page that fits.");
                    var group = body as RTools.UI.ContentGroup ?? (body is UserControl ? body.Controls.OfType<RTools.UI.ContentGroup>().SingleOrDefault() : null);
                    if (group != null)
                    {
                        var last = group.Controls.Cast<Control>().Last(c => c.Visible);
                        Check(last.Bottom == group.Height, name + ": extra blank area beneath the last card.");
                    }
                    else if (body is TableLayoutPanel dashboard)
                    {
                        Check(dashboard.Controls.Cast<Control>().Where(c => c.Visible).Max(c => c.Bottom) == dashboard.Height, name + ": extra space beneath the last dashboard row.");
                    }
                    host.ScrollTo(false, 0); Application.DoEvents();
                }
                foreach (var suffix in new[] { "Home", "Join", "Search", "Report", "Compare", "Rename", "Csv", "Subnet", "Log", "SystemInfo", "Folder", "Regex", "Color" })
                {
                    ((Button)form.Controls.Find("nav" + suffix, true).Single()).PerformClick();
                    AssertBounded(form.ActiveTool);
                }
                ((Button)form.Controls.Find("aboutButton", true).Single()).PerformClick();
                AssertBounded("About & Help");
                var guide = CurrentPage().Controls.Cast<Control>().SelectMany(Descendants).OfType<RTools.UI.ModernChoice>().Single();
                guide.SelectedIndex = 4; AssertBounded("About & Help: different guide");
                form.ClientSize = new Size(1300, 850); AssertBounded("About & Help: wider window");
                form.Close();
                IEnumerable<Control> Descendants(Control c) => new[] { c }.Concat(c.Controls.Cast<Control>().SelectMany(Descendants));
            }
            catch (Exception ex) { error = ex; }
        });
        thread.IsBackground = true; thread.SetApartmentState(ApartmentState.STA); thread.Start();
        Check(thread.Join(TimeSpan.FromSeconds(60)), "Page layout regression timed out.");
        if (error != null) throw new Exception("Page layout regression failed: " + error.Message, error);
    });
    Test("Content measurement shrinks wrapped labels and preserves fixed editor height", () =>
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = new Form { ClientSize = new Size(500, 250) };
                var page = RTools.UI.Ui.Page(out var body);
                var label = RTools.UI.Ui.Note(string.Join(" ", Enumerable.Repeat("Example line of wrapped text.", 100)));
                var edit = RTools.UI.Ui.Rich("Example", 100);
                RTools.UI.Ui.Add(body, RTools.UI.Ui.Card("Test", "", label, edit));
                var host = new RTools.UI.ModernScrollHost(page);
                form.Controls.Add(host); form.Show(); Application.DoEvents();
                Check(RTools.UI.PageContentLayout.TryGet(page, out var layout), "Page layout missing.");
                layout.Refresh(); Application.DoEvents();
                int tall = layout.ContentBottom;
                Check(layout.Maximum > 0, "Long content did not overflow.");
                host.ScrollTo(false, int.MaxValue); Application.DoEvents();
                label.Text = "Short text"; Application.DoEvents(); layout.Refresh(); Application.DoEvents();
                Check(layout.ContentBottom < tall && layout.Maximum == 0 && page.AutoScrollPosition.Y == 0 && !page.VerticalScroll.Visible, "Shortened content left a stale scroll range.");
                Check(edit.Parent.Height == 100, "Editor height changed with content or scroll position.");
                form.Close();
            }
            catch (Exception ex) { error = ex; }
        });
        thread.IsBackground = true; thread.SetApartmentState(ApartmentState.STA); thread.Start();
        Check(thread.Join(TimeSpan.FromSeconds(30)), "Content shrink regression timed out.");
        if (error != null) throw new Exception("Content shrink regression failed: " + error.Message, error);
    });
    Test("Rounded scroll thumb geometry handles limits and proportional sizing", () =>
    {
        var top = RTools.UI.ModernScrollHost.ThumbBounds(206, 16, 24, 200, 600, 0, 3);
        var bottom = RTools.UI.ModernScrollHost.ThumbBounds(206, 16, 24, 200, 600, 600, 3);
        var middle = RTools.UI.ModernScrollHost.ThumbBounds(206, 16, 24, 200, 600, 300, 3);
        Check(top.Y == 3 && top.Height == 50 && bottom.Bottom == 203 && middle.Y == 78, "Proportional thumb sizing or endpoints failed.");
        Check(RTools.UI.ModernScrollHost.ThumbBounds(206, 16, 24, 200, 0, 0, 3).Height == 200, "No-overflow geometry failed.");
        Check(RTools.UI.ModernScrollHost.ThumbBounds(14, 16, 24, 10, 500, 500, 3).Height == 8, "Tiny viewport clamp failed.");
    });
    Test("Modern choices preserve selections and handle item changes", () =>
    {
        Exception? error = null;
        var thread = new Thread(() => { try {
        using var choice = RTools.UI.Ui.Choice("Alpha", "Beta", "Gamma");
        int changes = 0; choice.SelectedIndexChanged += (_, _) => changes++;
        Check(choice.SelectedIndex == 0 && (string)choice.SelectedItem! == "Alpha", "Default selection is wrong.");
        choice.SelectedIndex = 2;
        Check((string)choice.SelectedItem! == "Gamma" && changes == 1, "Selection change was not raised.");
        choice.Items.Insert(0, "First");
        Check(choice.SelectedIndex == 3 && (string)choice.SelectedItem! == "Gamma", "Insertion did not preserve selection.");
        choice.Items.Clear();
        Check(choice.SelectedIndex == -1 && choice.SelectedItem == null, "Clearing left a stale selection.");
        choice.Items.AddRange(new[] { "All columns", "Name", "Name" });
        choice.SelectedIndex = 2;
        Check(choice.SelectedIndex == 2, "Duplicate labels lost their separate indices.");
        Throws<ArgumentOutOfRangeException>(() => choice.SelectedIndex = 99);
        } catch (Exception ex) { error = ex; } });
        thread.IsBackground = true; thread.SetApartmentState(ApartmentState.STA); thread.Start();
        Check(thread.Join(TimeSpan.FromSeconds(15)), "Choice selection test timed out.");
        if (error != null) throw new Exception("Choice selection test failed.", error);
    });
    Test("Modern scroll hosts and long dropdowns use the native scroll ranges on Windows", () =>
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = new Form { ClientSize = new Size(700, 530) };
                var list = RTools.UI.Ui.Files(120); list.HorizontalExtent = 1600;
                for (int i = 0; i < 200; i++) list.Items.Add(i + new string('X', 180));
                var listHost = new RTools.UI.ModernScrollHost(list) { Dock = DockStyle.None, Bounds = new Rectangle(10, 10, 310, 125) };
                var grid = RTools.UI.Ui.Grid(150); RTools.UI.Ui.Column(grid, "Wide column", "C", 900);
                for (int i = 0; i < 200; i++) grid.Rows.Add("Row " + i);
                var gridHost = new RTools.UI.ModernScrollHost(grid) { Dock = DockStyle.None, Bounds = new Rectangle(10, 145, 310, 150) };
                var text = RTools.UI.Ui.Box(string.Join("\r\n", Enumerable.Range(0, 200)), true, 100);
                var textHost = new RTools.UI.ModernScrollHost(text) { Dock = DockStyle.None, Bounds = new Rectangle(10, 305, 310, 100) };
                var choice = RTools.UI.Ui.Choice(Enumerable.Range(1, 60).Select(i => "Option " + i).ToArray());
                choice.Bounds = new Rectangle(345, 10, 300, 34);
                form.Controls.AddRange(new Control[] { listHost, gridHost, textHost, choice });
                form.Show(); Application.DoEvents();
                foreach (var host in new[] { listHost, gridHost, textHost })
                {
                    host.Synchronize();
                    Check(host.Controls.Find("modernVerticalScrollBar", false).Single().Visible, "Vertical rail did not appear.");
                }
                foreach (var host in new[] { listHost, gridHost })
                    Check(host.Controls.Find("modernHorizontalScrollBar", false).Single().Visible, "Horizontal rail did not appear.");
                listHost.ScrollTo(false, 40); Check(list.TopIndex == 40, "List vertical scrolling failed.");
                gridHost.ScrollTo(false, 40); Check(grid.FirstDisplayedScrollingRowIndex == 40, "Grid vertical scrolling failed.");
                gridHost.ScrollTo(true, 200); Check(grid.HorizontalScrollingOffset == 200, "Grid horizontal scrolling failed.");
                textHost.ScrollTo(false, 30); Check(text.GetCharIndexFromPosition(new Point(2, 2)) > 0, "Text editor did not scroll.");
                choice.SelectedIndex = 12; choice.OpenPopup(); Application.DoEvents();
                Check(choice.DroppedDown, "Long option popup did not open.");
                choice.ClosePopup(); Check(!choice.DroppedDown && choice.SelectedIndex == 12, "Closing changed the selection.");
                form.Close();
            }
            catch (Exception ex) { error = ex; }
        });
        thread.IsBackground = true; thread.SetApartmentState(ApartmentState.STA); thread.Start();
        Check(thread.Join(TimeSpan.FromSeconds(45)), "Scroll/dropdown smoke test timed out.");
        if (error != null) throw new Exception("Scroll/dropdown smoke test failed.", error);
    });
    Test("Modern numeric fields and sliders preserve bounded values and change events", () =>
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = new Form { ClientSize = new Size(500, 180) };
                using var number = RTools.UI.Ui.Number(128, 0, 255);
                using var slider = new RTools.UI.ModernSlider();
                number.Bounds = new Rectangle(12, 12, 120, 34);
                slider.Bounds = new Rectangle(155, 12, 300, 34);
                form.Controls.AddRange(new Control[] { number, slider });
                form.Show(); Application.DoEvents();
                var edit = number.Controls.OfType<TextBox>().Single();
                int numberChanges = 0, sliderChanges = 0;
                number.ValueChanged += (_, _) => numberChanges++;
                slider.ValueChanged += (_, _) => sliderChanges++;
                Check(number.Value == 128 && edit.Text == "128", "Numeric initial value failed.");
                edit.Text = "201";
                Check(number.Value == 201 && numberChanges == 1, "Typing a valid value did not synchronize.");
                edit.Text = "invalid";
                Check(number.Value == 201, "Invalid input changed the value.");
                number.Value = 999;
                Check(number.Value == 255 && edit.Text == "255", "Numeric upper limit failed.");
                number.Value = -1;
                Check(number.Value == 0 && edit.Text == "0", "Numeric lower limit failed.");
                slider.Value = 128; slider.Value = 128;
                Check(slider.Value == 128 && sliderChanges == 1, "Slider raised a redundant event.");
                slider.Value = 1000; Check(slider.Value == 255, "Slider upper limit failed.");
                slider.Value = -1; Check(slider.Value == 0, "Slider lower limit failed.");
                bool previous = RTools.UI.Theme.Dark;
                try
                {
                    foreach (bool dark in new[] { true, false })
                    {
                        RTools.UI.Theme.Dark = dark; RTools.UI.Theme.Apply(form);
                        using var numberBitmap = new Bitmap(number.Width, number.Height);
                        using var sliderBitmap = new Bitmap(slider.Width, slider.Height);
                        number.DrawToBitmap(numberBitmap, number.ClientRectangle);
                        slider.DrawToBitmap(sliderBitmap, slider.ClientRectangle);
                        Check(number.Value == 0 && slider.Value == 0, "Theme change altered input values.");
                    }
                }
                finally { RTools.UI.Theme.Dark = previous; }
                form.Close();
            }
            catch (Exception ex) { error = ex; }
        });
        thread.IsBackground = true; thread.SetApartmentState(ApartmentState.STA); thread.Start();
        Check(thread.Join(TimeSpan.FromSeconds(30)), "Input control regression timed out.");
        if (error != null) throw new Exception("Input control regression failed.", error);
    });
    Test("Application icon embeds desktop and high-DPI sizes", () =>
    {
        using var stream = typeof(RTools.MainForm).Assembly.GetManifestResourceStream("RTools.Assets.hammericon.ico");
        Check(stream != null, "Embedded icon missing.");
        using var reader = new BinaryReader(stream);
        Check(reader.ReadUInt16() == 0 && reader.ReadUInt16() == 1, "Invalid ICO header.");
        int count = reader.ReadUInt16();
        var sizes = new List<int>();
        for (int i = 0; i < count; i++)
        {
            int w = reader.ReadByte(), h = reader.ReadByte();
            w = w == 0 ? 256 : w; h = h == 0 ? 256 : h;
            Check(w == h, "Non-square ICO entry.");
            reader.ReadByte(); reader.ReadByte(); reader.ReadUInt16();
            Check(reader.ReadUInt16() == 32, "Icon must be 32-bit RGBA.");
            uint length = reader.ReadUInt32(), offset = reader.ReadUInt32();
            Check(length > 0 && (long)offset + length <= stream.Length, "Invalid icon frame range.");
            sizes.Add(w);
        }
        Check(sizes.OrderBy(n => n).SequenceEqual(new[] { 16, 20, 24, 32, 40, 48, 64, 96, 128, 256 }), "Missing desktop/DPI icon sizes.");
        foreach (int size in new[] { 16, 32, 48, 256 })
        {
            stream.Position = 0;
            using var icon = new Icon(stream, size, size);
            Check(icon.Width == size && icon.Height == size, "Windows could not load icon size " + size);
        }
    });
    Test("Folder analyzer totals nested files and largest files", () =>
    {
        var folder = Path.Combine(root, "analyze"); Directory.CreateDirectory(Path.Combine(folder, "nested"));
        File.WriteAllBytes(Path.Combine(folder, "one"), new byte[10]); File.WriteAllBytes(Path.Combine(folder, "nested", "two"), new byte[20]);
        var r = FolderAnalyzer.Scan(folder, 1); Check(r.Files == 2 && r.Folders == 1 && r.Bytes == 30 && r.Largest.Count == 1 && r.Largest[0].Bytes == 20 && r.Children[0].Bytes == 20, "Incorrect folder summary.");
    });
}
finally { try { Directory.Delete(root, true); } catch { } }
Console.WriteLine($"{passed} passed; {failed} failed.");
return failed == 0 ? 0 : 1;
