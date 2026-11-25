using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using RTools.Core;

namespace RTools.UI.Pages
{
    internal static class AboutHelpPageRuntime
    {
        private static readonly (string Name, string Instructions)[] Guides =
        {
            ("Text File Joiner", "1. Choose Add files or drop files onto the list. Use Move up and Move down to arrange a selected file.\r\n2. Choose the output file and number of blank lines between inputs. Leave Delete originals unchecked unless you intend to remove them.\r\n3. Click Join files. RTools writes the UTF-8 output before deleting any originals. Existing output can be replaced after confirmation.\r\nConsole: tfj (see Command line below)."),
            ("Search & Append", "1. Choose the search file and destination; enter one literal keyword per line. A match on any keyword selects the entire line.\r\n2. Choose Ignore case if needed. Click Preview matches to count the matching lines.\r\n3. Run the operation. Copy mode leaves the input intact; Move matching lines removes matches from it after appending to the destination and creates a backup.\r\nConsole: sa (moves matches; see Command line below)."),
            ("Report to CSV", "1. Select a Cardholder Transaction Detail Report and a separate CSV output path. This is a legacy fixed-width report parser, not a generic text-to-CSV converter.\r\n2. Review header lines and the six-digit transaction prefixes. Keep card/account masking enabled unless you are authorized to export identifiers.\r\n3. Click Preview to validate the recognized records, then Export CSV. Verify the output against a representative report before production use.\r\nGUI only."),
            ("File Comparison", "1. Browse for the original and modified file.\r\n2. Select Text for aligned line differences, or Binary for exact byte equality. Text mode can ignore case or whitespace.\r\n3. Click Compare files. Red rows identify removed text; green rows identify additions. Changes only hides unchanged rows. Text comparison has file-size and line-count limits.\r\nGUI only."),
            ("Bulk Renamer", "1. Add the files you want to rename. File extensions are preserved.\r\n2. Set find/replace (literal or regex), case conversion, prefix, numbering and suffix. Numbering follows sorted full-path order.\r\n3. Preview the batch and review every proposed filename. Resolve conflicts before clicking Apply reviewed batch. Existing files are not overwritten. Undo last batch restores names from the most recent successful batch in this session.\r\nGUI only."),
            ("CSV Workbench", "1. Select a CSV or delimited text file, choose the delimiter and whether the first row contains headers, then Load CSV. Merge another file requires matching headers.\r\n2. Edit cells, sort with column headers, filter the visible rows, add/delete rows or remove duplicates. The input files are not changed.\r\n3. Select export columns, choose a separate output file, and Export visible rows. Keep spreadsheet-safe export enabled to neutralize formula-like content.\r\nGUI only."),
            ("Subnet Calculator", "1. Enter an IPv4 address, such as 192.168.1.42, and a prefix length or dotted mask, such as 24 or 255.255.255.0. You can also enter 192.168.1.42/24 directly; the embedded prefix takes precedence.\r\n2. Click Calculate to see the network, broadcast, mask, wildcard, usable range and host counts.\r\n3. Use Copy results to copy the calculation. IPv4 only; /31 and /32 are supported.\r\nGUI only."),
            ("Log Viewer", "1. Choose a log file. Optionally enter text, select severity and set the displayed line limit.\r\n2. Click Read log for a snapshot. Error and warning entries are highlighted.\r\n3. Enable Follow log to check for changes every two seconds while the page is visible. The viewer reads up to the most recent 1 MiB; large logs are assumed to be UTF-8. It does not edit logs.\r\nGUI only."),
            ("System Information", "1. Click Refresh to collect local Windows, processor, memory, uptime, storage and active network-adapter details.\r\n2. Use Copy all or Save report to export the results. No elevation is required for normal collection.\r\n3. Review the report for device names, IP addresses and other identifying information before sharing it.\r\nGUI only."),
            ("Folder Analyzer", "1. Select a folder and the number of largest files you want to retain in the results.\r\n2. Click Analyze folder. Cancel from the status bar if necessary.\r\n3. Review direct-child totals, total files and folders, and the largest files. Sizes are logical file sizes; inaccessible entries and junctions/symlinks are skipped. Nothing is deleted.\r\nGUI only."),
            ("Regex Tester", "1. Enter a .NET regular expression and sample text. For example, \\b\\d+\\b matches whole numbers.\r\n2. Choose Ignore case, Multiline or Singleline if needed. Enter a replacement expression and enable Preview replacement to test substitutions.\r\n3. Click Run regex to see highlighted matches, positions and capture groups. Review and copy the replacement preview. Match counts, input size and execution time are bounded.\r\nGUI only."),
            ("Color Picker", "1. Move the RGB sliders or enter channel values from 0 to 255. The color swatch updates as you change them.\r\n2. Enter a HEX value and click Apply HEX, enter HSL values and click Apply HSL, or use Choose color to open the Windows color dialog.\r\n3. Copy HEX, RGB or HSL for use in CSS and other applications. HSL hue is 0–360 degrees, saturation and lightness are percentages.\r\nGUI only.")
        };

internal static Panel Build(AboutHelpPage view, Func<Task> checkUpdates, Action<string> openTool)
        {
        var ui = DesignerPageRuntime.Attach(view);
        var page = ui.Page;
        ui.Get<Label>("versionHeading").Text = "RTools " + MainForm.DisplayVersion;
            
            var guideChoice = ui.Get<ModernChoice>("guideChoice");
            
            var instructions = ui.Get<Label>("instructions");
            var initialGuide = Guides.FirstOrDefault(item => item.Name == guideChoice.SelectedItem?.ToString());
            if (initialGuide.Name != null) instructions.Text = initialGuide.Instructions;
            guideChoice.SelectedIndexChanged += (_, _) =>
            {
                var guide = Guides.FirstOrDefault(item => item.Name == guideChoice.SelectedItem?.ToString());
                if (guide.Name != null) instructions.Text = guide.Instructions;
            };
            ui.BindClick("open", () =>
            {
                var guide = Guides.FirstOrDefault(item => item.Name == guideChoice.SelectedItem?.ToString());
                if (guide.Name != null) openTool(guide.Name);
            });
            ui.Get<TextBox>("tfj").Text = ConsoleHelp.JoinSyntax;
            ui.Get<TextBox>("tfjExample").Text = ConsoleHelp.JoinExample;
            ui.Get<TextBox>("sa").Text = ConsoleHelp.SearchSyntax;
            ui.Get<TextBox>("saExample").Text = ConsoleHelp.SearchExample;
            
            ui.Get<Label>("joinTextFilesInArgumentOrderBlankLabel").Text = ConsoleHelp.JoinDescription;
            ui.Get<Label>("moveEntireLinesContainingAnyLiteralLabel").Text = ConsoleHelp.SearchDescription;
            
            ui.BindAsync("checkForUpdatesButton", checkUpdates);
        ui.BindClick("githubRepositoryButton", () => UpdateChecker.Open(UpdateChecker.Repository));
        ui.BindClick("releasesbutton", () => UpdateChecker.Open(UpdateChecker.Releases));
        ui.BindClick("copyConsoleHelpButton", () => Ui.Copy(ConsoleHelp.Usage(MainForm.DisplayVersion)));
        ui.BindClick("copyTfjExampleButton", () => Ui.Copy(ConsoleHelp.JoinExample));
        ui.BindClick("copySaExampleButton", () => Ui.Copy(ConsoleHelp.SearchExample));
            return page;
        }
    }
}
