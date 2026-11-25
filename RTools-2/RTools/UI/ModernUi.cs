using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RTools.UI;

internal static class Theme
{
    public static bool Dark = true;
    public static Color Primary => Color.FromArgb(134,27,45);
    public static Color Accent => Color.FromArgb(248,51,87);
    public static Color Bg => Dark ? Color.FromArgb(31,31,31) : Color.FromArgb(246,246,246);
    public static Color Sidebar => Dark ? Color.FromArgb(38,38,38) : Color.FromArgb(239,239,239);
    public static Color Surface => Dark ? Color.FromArgb(43,43,43) : Color.White;
    public static Color Input => Dark ? Color.FromArgb(52,52,52) : Color.FromArgb(250,250,250);
    public static Color Border => Dark ? Color.FromArgb(70,70,70) : Color.FromArgb(215,215,215);
    public static Color Ink => Dark ? Color.FromArgb(246,246,246) : Color.FromArgb(29,29,29);
    public static Color Muted => Dark ? Color.FromArgb(183,183,183) : Color.FromArgb(92,92,92);
    public static Color Highlight => Dark ? Color.FromArgb(88,38,50) : Color.FromArgb(249,225,230);
    public static Color SelectedText => Dark ? Color.FromArgb(255,160,179) : Primary;
    public static Color Removed => Dark ? Color.FromArgb(83,38,46) : Color.FromArgb(255,222,228);
    public static Color Added => Dark ? Color.FromArgb(32,66,46) : Color.FromArgb(217,244,226);
    public static Color Warning => Dark ? Color.FromArgb(89,67,26) : Color.FromArgb(255,243,203);
    public static void Apply(Control control)
    {
        var tag = control.Tag as string;
        if (control is DataGridView grid)
        {
            grid.BackgroundColor = Surface; grid.GridColor = Border; grid.EnableHeadersVisualStyles = false;
            grid.DefaultCellStyle.BackColor = Surface; grid.DefaultCellStyle.ForeColor = Ink;
            grid.DefaultCellStyle.SelectionBackColor = Highlight; grid.DefaultCellStyle.SelectionForeColor = Ink;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Dark ? Color.FromArgb(47,47,47) : Color.FromArgb(248,248,248);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Input; grid.ColumnHeadersDefaultCellStyle.ForeColor = Ink;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Input; grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Ink;
            grid.RowHeadersDefaultCellStyle.BackColor = Input; grid.RowHeadersDefaultCellStyle.ForeColor = Ink;
            grid.RowHeadersDefaultCellStyle.SelectionBackColor = Highlight; grid.RowHeadersDefaultCellStyle.SelectionForeColor = Ink;
        }
        else if (control is ModernButton || control is ModernNumber || control is ModernSlider) { control.ForeColor = Ink; control.Invalidate(); }
        else if (control is Button button) { button.BackColor = button.Parent?.BackColor ?? Bg; button.ForeColor = Ink; button.FlatAppearance.BorderColor = Border; button.Invalidate(); }
        else if (control is TextBoxBase || control is ListBox || control is ModernChoice || control is ComboBox || control is NumericUpDown)
        {
            control.BackColor = Input; control.ForeColor = Ink;
        }
        else if (control is Label) { control.ForeColor = tag == "accent" ? SelectedText : tag == "muted" ? Muted : Ink; control.BackColor = Color.Transparent; }
        else if (control is CheckBox) { control.ForeColor = Ink; control.BackColor = Color.Transparent; }
        else if (tag != "swatch") { control.BackColor = tag == "divider" ? Border : tag == "sidebar" ? Sidebar : tag == "surface" ? Surface : (tag == "inherit" || control is TrackBar) && control.Parent != null ? control.Parent.BackColor : Bg; control.ForeColor = Ink; }
        foreach (Control child in control.Controls) Apply(child);
        control.Invalidate();
    }
    public static string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RTools", "theme.txt");
    public static void Load() { try { if (File.Exists(SettingsPath)) Dark = File.ReadAllText(SettingsPath).Trim() != "light"; } catch { } }
    public static void Save() { try { Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!); File.WriteAllText(SettingsPath, Dark ? "dark" : "light"); } catch { } }
    [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
    public static void Titlebar(Form form)
    {
        try { int dark = Dark ? 1 : 0, corner = 2; if (DwmSetWindowAttribute(form.Handle, 20, ref dark, 4) != 0) DwmSetWindowAttribute(form.Handle, 19, ref dark, 4); DwmSetWindowAttribute(form.Handle, 33, ref corner, 4); } catch { }
    }
}

internal static class Ui
{
    public const string All = "All files (*.*)|*.*";
    public const string Csv = "CSV files (*.csv)|*.csv|Text files (*.txt;*.tsv)|*.txt;*.tsv|All files (*.*)|*.*";
    public static GraphicsPath Round(Rectangle r, int radius)
    {
        var path = new GraphicsPath(); int d = Math.Max(1,Math.Min(radius*2,Math.Min(r.Width,r.Height)));
        path.AddArc(r.X,r.Y,d,d,180,90); path.AddArc(r.Right-d,r.Y,d,d,270,90); path.AddArc(r.Right-d,r.Bottom-d,d,d,0,90); path.AddArc(r.X,r.Bottom-d,d,d,90,90); path.CloseFigure(); return path;
    }
    public static Label Label(string text, float size = 10, bool bold = false, string? role = null)
    {
        return new Label { Text = text, Font = new Font("Segoe UI",size,bold ? FontStyle.Bold : FontStyle.Regular), AutoSize=true, Tag=role, Margin=new Padding(0,0,0,7), UseMnemonic=false };
    }
    public static Label Note(string text) => Label(text,9,false,"muted");
    public static TextBox Box(string text = "", bool multi = false, int height = 100, bool readOnly = false) => new TextBox { Text=text, Multiline=multi, ReadOnly=readOnly, ScrollBars=multi ? ScrollBars.Both : ScrollBars.None, WordWrap=multi, Height=multi ? height : 30, BorderStyle=BorderStyle.FixedSingle, Margin=new Padding(0,0,0,8), Font=new Font("Segoe UI",10) };
    public static RichTextBox Rich(string text="", int height=155, bool readOnly=false) => new RichTextBox { Text=text, Height=height, BorderStyle=BorderStyle.FixedSingle, DetectUrls=false, ReadOnly=readOnly, WordWrap=false, ScrollBars=RichTextBoxScrollBars.Both, Font=new Font("Consolas",10), Margin=new Padding(0,0,0,8) };
    public static CheckBox Check(string text, bool value=false) => new ModernCheckBox { Text=text, Checked=value, AutoSize=true, Margin=new Padding(0,4,15,9), UseMnemonic=false };
    public static ModernChoice Choice(params string[] items) { var c = new ModernChoice(); c.Items.AddRange(items); if(items.Length>0)c.SelectedIndex=0; return c; }
    public static ModernNumber Number(int value, int min, int max) => new ModernNumber(value,min,max);
    public static ModernButton Button(string text, Action action, bool primary=false) { var b = new ModernButton { Text=text,PrimaryAction=primary }; b.Click += (_,_) => { try { action(); } catch(Exception ex) { Error(b,ex); } }; return b; }
    public static ModernButton Async(string text, Func<Task> action, bool primary=false) { var b = new ModernButton { Text=text,PrimaryAction=primary }; b.Click += async (_,_) => { try { await action(); } catch(Exception ex) { Error(b,ex); } }; return b; }
    public static void Error(Control owner, Exception ex) { if (!owner.IsDisposed) MessageBox.Show(owner.FindForm(),ex.Message,"RTools",MessageBoxButtons.OK,MessageBoxIcon.Error); }
    public static bool Confirm(Control owner, string text) => MessageBox.Show(owner.FindForm(),text,"RTools",MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2)==DialogResult.Yes;
    public static void Copy(string text) { if (text.Length > 0) Clipboard.SetText(text); }
    public static bool Overwrite(Control owner,string path) => !File.Exists(path) || Confirm(owner,"Replace the existing file?\n\n"+path);
    public static ContentGroup Stack(string role="inherit", int padding=0)
        => new ContentGroup(ContentDirection.Stack) { Padding = new Padding(padding), Tag = role };
    public static void Add(ContentGroup p, params Control[] controls)
    {
        foreach (var original in controls) p.AddItem(ModernScrollHost.Wrap(original));
    }
    public static Control Field(string caption, Control control) { if(string.IsNullOrEmpty(control.AccessibleName)) control.AccessibleName=caption; if(control is ModernNumber number) number.SetLabel(caption); var p=Stack(); Add(p,Label(caption,9,true),control); p.Margin=new Padding(0,0,0,6); return p; }
    public static Control Row(params Control[] controls)
    {
        var p = new ContentGroup(ContentDirection.Row) { Margin = new Padding(0,0,0,4) };
        Add(p, controls); return p;
    }
    public static Control Columns(params Control[] controls)
    {
        var p = new ContentGroup(ContentDirection.Columns) { Margin = new Padding(0,0,0,6) };
        for (int i = 0; i < controls.Length; i++)
        {
            var c = ModernScrollHost.Wrap(controls[i]);
            c.Margin = new Padding(0,0,i == controls.Length-1 ? 0 : 12,0);
            p.AddItem(c);
        }
        return p;
    }
    public static Control Card(string title,string desc,params Control[] controls)
    {
        var p=Stack("surface",16); p.Margin=new Padding(0,0,0,12);
        p.Paint += (_,e) => { using var pen=new Pen(Theme.Border); using var path=Round(new Rectangle(0,0,Math.Max(1,p.Width-1),Math.Max(1,p.Height-1)),8); e.Graphics.DrawPath(pen,path); };
        if(title.Length!=0)Add(p,Label(title,12,true)); if(desc.Length!=0)Add(p,Note(desc)); Add(p,controls); return p;
    }
    public static Panel Page(out ContentGroup body)
    {
        var p = new Panel { Dock=DockStyle.Fill, AutoScroll=true }; body=Stack("inherit");
        p.Controls.Add(body); PageContentLayout.Attach(p, body, new Padding(24,12,24,PageContentLayout.BottomPadding)); return p;
    }
    public static Control PathField(string label,TextBox path,bool save=false,string filter=All,bool folder=false)
    {
        var p = new ContentGroup(ContentDirection.Columns, 1);
        path.Margin=new Padding(0,3,8,7); p.AddItem(ModernScrollHost.Wrap(path));
        var b=Button("Browse…",()=>{ if(folder){using var d=new FolderBrowserDialog { Description=label,ShowNewFolderButton=false }; if(d.ShowDialog(path.FindForm())==DialogResult.OK)path.Text=d.SelectedPath;} else if(save){using var d=new SaveFileDialog {Filter=filter,OverwritePrompt=false,AddExtension=true}; if(d.ShowDialog(path.FindForm())==DialogResult.OK)path.Text=d.FileName;} else {using var d=new OpenFileDialog {Filter=filter,CheckFileExists=true};if(d.ShowDialog(path.FindForm())==DialogResult.OK)path.Text=d.FileName;} }); p.AddItem(b);
        path.AllowDrop=!save; path.DragEnter+=(_,e)=>{if(e.Data.GetDataPresent(DataFormats.FileDrop))e.Effect=DragDropEffects.Copy;};path.DragDrop+=(_,e)=>{if(e.Data?.GetData(DataFormats.FileDrop) is string[] a && a.Length>0)path.Text=a[0];};return Field(label,p);
    }
    public static ListBox Files(int height=120) => new ModernListBox { Height=height,SelectionMode=SelectionMode.MultiExtended,HorizontalScrollbar=true,AllowDrop=true,BorderStyle=BorderStyle.FixedSingle,Margin=new Padding(0,0,0,8) };
    public static void AddFiles(ListBox list,IEnumerable<string> paths) { foreach(var p in paths.Where(File.Exists))if(list.Items.Count<10000 && !list.Items.Cast<string>().Contains(p,StringComparer.OrdinalIgnoreCase))list.Items.Add(p); if(list is ModernListBox themed) themed.RefreshExtent(); }
    public static Control FileList(ListBox list,Action changed,bool order=false)
    {
        void AddPaths(IEnumerable<string> paths){AddFiles(list,paths);changed();}
        list.DragEnter+=(_,e)=>{if(e.Data.GetDataPresent(DataFormats.FileDrop))e.Effect=DragDropEffects.Copy;};list.DragDrop+=(_,e)=>{if(e.Data.GetData(DataFormats.FileDrop) is string[] a)AddPaths(a);};
        var buttons=new List<Control>{Button("Add files…",()=>{using var d=new OpenFileDialog{Multiselect=true,Filter=All};if(d.ShowDialog(list.FindForm())==DialogResult.OK)AddPaths(d.FileNames);}),Button("Remove selected",()=>{foreach(var item in list.SelectedItems.Cast<string>().ToArray())list.Items.Remove(item);if(list is ModernListBox themed)themed.RefreshExtent();changed();}),Button("Clear",()=>{list.Items.Clear();if(list is ModernListBox themed)themed.RefreshExtent();changed();})};
        void Move(int step){if(list.SelectedItems.Count!=1)return;int i=list.SelectedIndex,j=i+step;if(j<0||j>=list.Items.Count)return;var item=list.Items[i];list.Items.RemoveAt(i);list.Items.Insert(j,item);list.SelectedIndex=j;changed();}
        if(order){buttons.Add(Button("Move up",()=>Move(-1)));buttons.Add(Button("Move down",()=>Move(1)));}var p=Stack();Add(p,Row(buttons.ToArray()),list);return p;
    }
    public static DataGridView Grid(int height=270,bool editable=false)
    {
        var g=new DataGridView { Height=height,ReadOnly=!editable,AllowUserToAddRows=false,AllowUserToDeleteRows=false,AllowUserToResizeRows=false,AutoGenerateColumns=false,RowHeadersVisible=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=true,BorderStyle=BorderStyle.FixedSingle,CellBorderStyle=DataGridViewCellBorderStyle.SingleHorizontal,ColumnHeadersBorderStyle=DataGridViewHeaderBorderStyle.Single,ColumnHeadersHeight=32,ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.DisableResizing,RowTemplate={Height=27},AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.None,ClipboardCopyMode=DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText,Margin=new Padding(0,0,0,8) };
        g.EditingControlShowing += (_, e) => Theme.Apply(e.Control);
        typeof(DataGridView).GetProperty("DoubleBuffered",BindingFlags.Instance|BindingFlags.NonPublic)?.SetValue(g,true,null); return g;
    }
    public static void Column(DataGridView g,string title,string property,int width=0)
    {
        g.Columns.Add(new DataGridViewTextBoxColumn{HeaderText=title,DataPropertyName=property,Name=property,Width=width==0?100:width,MinimumWidth=45,AutoSizeMode=width==0?DataGridViewAutoSizeColumnMode.Fill:DataGridViewAutoSizeColumnMode.None,SortMode=DataGridViewColumnSortMode.NotSortable});
    }
    public static void Bind<T>(DataGridView g,IEnumerable<T> items) { g.DataSource=null;g.DataSource=items.ToList(); if(g.Rows.Count>0)g.ClearSelection(); }
}
