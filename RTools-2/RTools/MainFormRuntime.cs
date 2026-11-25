using System.ComponentModel;
using System.Reflection;
using RTools.UI;
using RTools.UI.Pages;

namespace RTools
{

internal sealed class MainFormRuntime
{
    private sealed record Tool(string Name, string Group, string Glyph, string Description, Func<Panel> Build);
    private bool runtimeInitialized;
    private readonly List<Tool> tools = new();
    private readonly Dictionary<string,Panel> pages = new();
    private readonly Dictionary<string,Button> navButtons = new();
    private readonly Dictionary<Button, ShellButtonStyle> buttonStyles = new();
    private Panel? active;
    private string selected = "Home";
    private CancellationTokenSource? operation;
    private readonly CancellationTokenSource lifetime = new();
    private bool updateRunning, closing;
    internal bool Busy => operation != null;
    internal string ActiveTool => selected;
    internal CancellationToken Lifetime => lifetime.Token;


    private readonly MainForm window;
    private readonly bool checkUpdatesOnStartup;
    private readonly Button themeButton;
    private readonly Button updater;
    private readonly Button aboutButton;
    private readonly Button cancel;
    private readonly TextBox search;
    private readonly Label versionLabel;
    private readonly Label status;
    private readonly FlowLayoutPanel nav;
    private readonly TableLayoutPanel side;
    private readonly Panel homePage;
    private readonly Panel content;
    private readonly Label pageTitle;
    private readonly Label pageSubtitle;
    private readonly TableLayoutPanel workspace;
    private readonly TableLayoutPanel heading;
    private readonly Label navFilesHeader;
    private readonly Label navItHeader;
    private readonly Label navDeveloperHeader;
    private readonly Button navHome;
    private readonly Button navJoin;
    private readonly Button navSearch;
    private readonly Button navReport;
    private readonly Button navCompare;
    private readonly Button navRename;
    private readonly Button navCsv;
    private readonly Button navSubnet;
    private readonly Button navLog;
    private readonly Button navSystemInfo;
    private readonly Button navFolder;
    private readonly Button navRegex;
    private readonly Button navColor;
    private readonly Button homeJoin;
    private readonly Button homeSearch;
    private readonly Button homeReport;
    private readonly Button homeCompare;
    private readonly Button homeRename;
    private readonly Button homeCsv;
    private readonly Button homeSubnet;
    private readonly Button homeLog;
    private readonly Button homeSystemInfo;
    private readonly Button homeFolder;
    private readonly Button homeRegex;
    private readonly Button homeColor;

    internal MainFormRuntime(MainForm window, bool checkUpdatesOnStartup)
    {
        this.window = window;
        this.checkUpdatesOnStartup = checkUpdatesOnStartup;
        themeButton = Find<Button>("themeButton");
        updater = Find<Button>("updater");
        aboutButton = Find<Button>("aboutButton");
        cancel = Find<Button>("cancel");
        search = Find<TextBox>("search");
        versionLabel = Find<Label>("versionLabel");
        status = Find<Label>("status");
        nav = Find<FlowLayoutPanel>("nav");
        side = Find<TableLayoutPanel>("side");
        homePage = Find<Panel>("homePage");
        content = Find<Panel>("content");
        pageTitle = Find<Label>("pageTitle");
        pageSubtitle = Find<Label>("pageSubtitle");
        workspace = Find<TableLayoutPanel>("workspace");
        heading = Find<TableLayoutPanel>("heading");
        navFilesHeader = Find<Label>("navFilesHeader");
        navItHeader = Find<Label>("navItHeader");
        navDeveloperHeader = Find<Label>("navDeveloperHeader");
        navHome = Find<Button>("navHome");
        navJoin = Find<Button>("navJoin");
        navSearch = Find<Button>("navSearch");
        navReport = Find<Button>("navReport");
        navCompare = Find<Button>("navCompare");
        navRename = Find<Button>("navRename");
        navCsv = Find<Button>("navCsv");
        navSubnet = Find<Button>("navSubnet");
        navLog = Find<Button>("navLog");
        navSystemInfo = Find<Button>("navSystemInfo");
        navFolder = Find<Button>("navFolder");
        navRegex = Find<Button>("navRegex");
        navColor = Find<Button>("navColor");
        homeJoin = Find<Button>("homeJoin");
        homeSearch = Find<Button>("homeSearch");
        homeReport = Find<Button>("homeReport");
        homeCompare = Find<Button>("homeCompare");
        homeRename = Find<Button>("homeRename");
        homeCsv = Find<Button>("homeCsv");
        homeSubnet = Find<Button>("homeSubnet");
        homeLog = Find<Button>("homeLog");
        homeSystemInfo = Find<Button>("homeSystemInfo");
        homeFolder = Find<Button>("homeFolder");
        homeRegex = Find<Button>("homeRegex");
        homeColor = Find<Button>("homeColor");
    }

    private T Find<T>(string name) where T : Control
    {
        var found = window.Controls.Find(name, true);
        if (found.Length != 1 || !(found[0] is T control))
            throw new InvalidOperationException("Missing or invalid main-window control: " + name);
        return control;
    }

    internal void OnLoad(object? sender, EventArgs e) => InitializeRuntime();

    private void InitializeRuntime()
    {
        if (runtimeInitialized) return;
        runtimeInitialized = true;
        Theme.Load();
        tools.AddRange(new Tool[]{
            new("Text File Joiner","FILES & DATA","\uE8A5","Combine multiple text files in the order you choose.",() => TextFileJoinerPageRuntime.Build(new TextFileJoinerPage(), window)),
            new("Search & Append","FILES & DATA","\uE721","Find, copy or move matching lines between files.",() => SearchAppendPageRuntime.Build(new SearchAppendPage(), window)),
            new("Report to CSV","FILES & DATA","\uE9F9","Extract fixed-width transaction reports to CSV.",() => ReportToCsvPageRuntime.Build(new ReportToCsvPage(), window)),
            new("File Comparison","FILES & DATA","\uE8F1","Compare text line by line or verify binary identity.",() => FileComparisonPageRuntime.Build(new FileComparisonPage(), window)),
            new("Bulk Renamer","FILES & DATA","\uE8AC","Preview and safely rename batches of files.",() => BulkRenamerPageRuntime.Build(new BulkRenamerPage(), window)),
            new("CSV Workbench","FILES & DATA","\uE80A","Edit, filter, merge and export CSV data.",() => CsvWorkbenchPageRuntime.Build(new CsvWorkbenchPage(), window)),
            new("Subnet Calculator","IT & DIAGNOSTICS","\uE968","Calculate IPv4 networks and address ranges.",() => SubnetCalculatorPageRuntime.Build(new SubnetCalculatorPage(), window)),
            new("Log Viewer","IT & DIAGNOSTICS","\uE9F5","Follow active logs and filter by severity.",() => LogViewerPageRuntime.Build(new LogViewerPage(), window)),
            new("System Information","IT & DIAGNOSTICS","\uE770","View Windows, hardware and network details.",() => SystemInformationPageRuntime.Build(new SystemInformationPage(), window)),
            new("Folder Analyzer","IT & DIAGNOSTICS","\uE8B7","Find large folders and files using disk space.",() => FolderAnalyzerPageRuntime.Build(new FolderAnalyzerPage(), window)),
            new("Regex Tester","DEVELOPER TOOLS","\uE943","Test expressions, groups and replacements.",() => RegexTesterPageRuntime.Build(new RegexTesterPage(), window)),
            new("Color Picker","DEVELOPER TOOLS","\uE790","Explore and convert RGB, HEX and HSL colors.",() => ColorPickerPageRuntime.Build(new ColorPickerPage(), window))
        });
        themeButton.Text = "Switch to " + (Theme.Dark ? "light" : "dark") + " mode";
        themeButton.Click += (_, _) => { Theme.Dark = !Theme.Dark; Theme.Save(); themeButton.Text = "Switch to " + (Theme.Dark ? "light" : "dark") + " mode"; Theme.Apply(window); Theme.Titlebar(window); };
        updater.Click += async (_, _) => await CheckUpdates(true);
        aboutButton.Click += (_, _) => ShowPage("About & Help");
        // The executable already embeds the multi-resolution hammer icon.
        // Extract it using Windows rather than requiring GDI+ to decode an ICO
        // stream during Load. An optional icon must never prevent launching.
        try
        {
            var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (icon != null)
            {
                window.Icon = icon;
                window.Disposed += (_, _) => icon.Dispose();
            }
        }
        catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is System.Runtime.InteropServices.ExternalException || ex is InvalidOperationException || ex is System.Security.SecurityException)
        {
            // Keep the default window icon if extraction is unavailable.
        }
        versionLabel.Text="v"+MainForm.DisplayVersion;search.AccessibleName="Find a tool";search.TextChanged+=(_,_)=>FilterNavigation();
        try { SendCueBanner(); } catch { }
        cancel.Click+=(_,_)=>{operation?.Cancel();cancel.Enabled=false;status.Text="Cancellation requested…";};
        // Keep all designer controls stock. Only the running shell wraps its scrollable regions.
        side.SuspendLayout();
        side.Controls.Remove(search);
        side.Controls.Add(new ModernInputFrame(search) { Dock = DockStyle.Fill }, 0, 1);
        side.Controls.Remove(nav);
        var navigationHost = new ModernScrollHost(nav) { Name = "navigationScrollHost", Tag = "sidebar" };
        side.Controls.Add(navigationHost, 0, 2);
        side.ResumeLayout(true);
        content.SuspendLayout();
        content.Controls.Remove(homePage);
        PageContentLayout.Attach(homePage, homePage.Controls["homeGrid"], new Padding(homePage.Padding.Left, homePage.Padding.Top, homePage.Padding.Right, PageContentLayout.BottomPadding));
        var homeHost = new ModernScrollHost(homePage) { Name = "homeScrollHost" };
        content.Controls.Add(homeHost);
        content.ResumeLayout(true);
        nav.SizeChanged+=(_,_)=>ResizeNavigation();
        WireNavigation();pages.Add("Home", homeHost);ShowPage("Home");Theme.Apply(window);Theme.Titlebar(window);
        if (checkUpdatesOnStartup) window.Shown += async (_,_) => await CheckUpdates(false);
        window.FormClosing+=(_,e)=>{ if(Busy){e.Cancel=true;operation?.Cancel();status.Text="Wait for the operation to finish before closing.";return;}closing=true;lifetime.Cancel();};
        window.KeyPreview=true;window.KeyDown+=(_,e)=>{if(e.Control && e.KeyCode==Keys.K){search.Focus();search.SelectAll();e.SuppressKeyPress=true;}else if(e.KeyCode==Keys.F1 && !Busy){ShowPage("About & Help");e.SuppressKeyPress=true;}else if(e.KeyCode==Keys.Escape && Busy){operation?.Cancel();e.SuppressKeyPress=true;}};
    }
    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet=System.Runtime.InteropServices.CharSet.Unicode)] private static extern IntPtr SendMessage(IntPtr hWnd,uint msg,IntPtr wParam,string lParam);
    private void SendCueBanner()=>SendMessage(search.Handle,0x1501,(IntPtr)1,"Find a tool  (Ctrl+K)");
    private void WireNavigation()
    {
        StyleButton(themeButton, ShellButtonKind.Navigation, "\uE708");
        StyleButton(updater, ShellButtonKind.Navigation, "\uE895");
        StyleButton(aboutButton, ShellButtonKind.Navigation, "\uE946");
        StyleButton(cancel, ShellButtonKind.Action, "");
        BindNavigation("Home", navHome, null);
        BindNavigation("Text File Joiner", navJoin, homeJoin);
        BindNavigation("Search & Append", navSearch, homeSearch);
        BindNavigation("Report to CSV", navReport, homeReport);
        BindNavigation("File Comparison", navCompare, homeCompare);
        BindNavigation("Bulk Renamer", navRename, homeRename);
        BindNavigation("CSV Workbench", navCsv, homeCsv);
        BindNavigation("Subnet Calculator", navSubnet, homeSubnet);
        BindNavigation("Log Viewer", navLog, homeLog);
        BindNavigation("System Information", navSystemInfo, homeSystemInfo);
        BindNavigation("Folder Analyzer", navFolder, homeFolder);
        BindNavigation("Regex Tester", navRegex, homeRegex);
        BindNavigation("Color Picker", navColor, homeColor);
        ResizeNavigation();
    }
    private void StyleButton(Button button, ShellButtonKind kind, string glyph)
    {
        buttonStyles.Add(button, new ShellButtonStyle(button, kind, glyph));
    }
    private void BindNavigation(string name, Button navButton, Button? homeButton)
    {
        string glyph = tools.FirstOrDefault(t => t.Name == name)?.Glyph ?? "\uE80F";
        StyleButton(navButton, ShellButtonKind.Navigation, glyph);
        navButton.Click += (_, _) => ShowPage(name);
        navButtons.Add(name, navButton);
        if (homeButton != null)
        {
            // The whole tile is a single native Button, not a button inside a panel.
            StyleButton(homeButton, ShellButtonKind.Card, glyph);
            homeButton.Click += (_, _) => ShowPage(name);
        }
    }
    private void ResizeNavigation()
    {
        // Reserve the scrollbar once, not once in ClientSize and again here.
        int reserved = Math.Max(SystemInformation.VerticalScrollBarWidth, nav.Width - nav.ClientSize.Width);
        int w = Math.Max(120, nav.Width - reserved - 4);
        foreach (var b in navButtons.Values) b.Width = w;
    }
    private void FilterNavigation()
    {
        if (!runtimeInitialized || navButtons.Count == 0) return;
        string term = search.Text.Trim();
        nav.SuspendLayout();
        foreach (var tool in tools)
            navButtons[tool.Name].Visible = term.Length == 0 || tool.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 || tool.Description.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 || tool.Group.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        foreach (var label in new[] { navFilesHeader, navItHeader, navDeveloperHeader })
            label.Visible = tools.Any(t => t.Group == label.Text && navButtons[t.Name].Visible);
        nav.ResumeLayout();
    }
    private void ShowPage(string name)
    {
        if(Busy)return;
        var tool=tools.FirstOrDefault(t=>t.Name==name);
        if(!pages.TryGetValue(name,out var panel))
        {
            var toolPage = name == "About & Help" ? About() : tool!.Build();
            panel = new ModernScrollHost(toolPage);
            pages.Add(name,panel);content.Controls.Add(panel);Theme.Apply(panel);
        }
        if(active!=null)active.Visible=false;active=panel;active.Visible=true;active.BringToFront();selected=name;
        pageTitle.Text = name;
        bool isHome = name == "Home";
        pageSubtitle.Text = isHome ? "" : tool?.Description ?? "Tool instructions, console commands, version and updates.";
        pageSubtitle.Visible = !isHome;
        // Collapse the subtitle area on Home; other pages retain their descriptions.
        float titleHeight = heading.Padding.Vertical + heading.RowStyles[0].Height;
        workspace.RowStyles[0].Height = titleHeight + (isHome ? 0 : (int)Math.Round(33 * window.DeviceDpi / 96.0));
        foreach(var b in navButtons) buttonStyles[b.Value].Selected=b.Key==name; buttonStyles[aboutButton].Selected=name=="About & Help";status.Text="Ready";
    }
    private Panel About()
    {
        return AboutHelpPageRuntime.Build(new AboutHelpPage(), () => CheckUpdates(true), ShowPage);
    }
    internal async Task Run<T>(Func<CancellationToken,IProgress<string>,T> work,Action<T> completed)
    {
        if(Busy||closing)return;var cts=new CancellationTokenSource();operation=cts;var page=active;
        if(page!=null)page.Enabled=false;side.Enabled=false;cancel.Visible=true;cancel.Enabled=true;status.Text="Working…";
        var progress=new Progress<string>(s=>{if(operation==cts&&!closing)status.Text=s;});
        try{var result=await Task.Run(()=>work(cts.Token,progress),cts.Token);if(!closing){completed(result);status.Text="Completed";}}
        catch(OperationCanceledException){if(!closing)status.Text="Cancelled";}
        catch(Exception ex){if(!closing){status.Text="Operation failed";Ui.Error(window,ex);}}
        finally{operation=null;cts.Dispose();if(!closing){if(page!=null)page.Enabled=true;side.Enabled=true;cancel.Visible=false;}}
    }
    private async Task CheckUpdates(bool manual)
    {
        if(updateRunning){if(manual)MessageBox.Show(window,"An update check is already running.","RTools Updater");return;}updateRunning=true;
        try
        {
            var latest=await UpdateChecker.Fetch(lifetime.Token);if(closing)return;
            if(UpdateChecker.IsNewer(latest,MainForm.CurrentVersion))
            {if(MessageBox.Show(window,$"RTools {latest} is available.\nInstalled version: {MainForm.DisplayVersion}.\n\nOpen the GitHub releases page to download it?","RTools Updater",MessageBoxButtons.YesNo,MessageBoxIcon.Information)==DialogResult.Yes)UpdateChecker.Open(UpdateChecker.Releases);}
            else if(manual)MessageBox.Show(window,$"You're up to date.\nInstalled: {MainForm.DisplayVersion}\nPublished: {latest}","RTools Updater",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }
        catch(Exception ex){if(manual&&!closing)MessageBox.Show(window,"Could not check for updates. The rest of RTools is still available.\n\n"+ex.Message,"RTools Updater",MessageBoxButtons.OK,MessageBoxIcon.Warning);}
        finally{updateRunning=false;}
    }
}
}
