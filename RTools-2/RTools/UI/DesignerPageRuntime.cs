using System.Reflection;

namespace RTools.UI;

// Design-time views use only built-in controls. On the explicit runtime path,
// adapt those controls to the theme and responsive layout. The designer is the
// single source of the control tree, text, order, margins and input dimensions.
internal sealed class DesignerPageRuntime
{
    private readonly Dictionary<string, Control> controls = new(StringComparer.Ordinal);
    internal Panel Page { get; }

    private DesignerPageRuntime(UserControl view)
    {
        LayoutTree(view);
        var originals = view.Controls.Cast<Control>().OrderBy(c => c.Top).ThenBy(c => c.Left).ToArray();
        view.SuspendLayout();
        view.Controls.Clear();
        var converted = originals.Select(Promote).ToArray();
        ContentGroup root;
        if (converted.Length == 1 && converted[0] is ContentGroup single) root = single;
        else
        {
            root = new ContentGroup(ContentDirection.Stack, designerSizing: true);
            foreach (var child in converted) root.AddItem(ModernScrollHost.Wrap(child));
        }
        root.Dock = DockStyle.None;
        root.Location = Point.Empty;
        view.Controls.Add(root);
        view.ResumeLayout(false);
        view.Margin = Padding.Empty;
        view.Tag = "inherit";
        Page = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Name = view.Name + "Viewport" };
        Page.Controls.Add(view);
        PageContentLayout.Attach(Page, view, new Padding(24, 12, 24, PageContentLayout.BottomPadding));
    }

    internal static DesignerPageRuntime Attach(UserControl view) => new(view);
    internal T Get<T>(string name) where T : Control => (T)controls[name];

    private static void LayoutTree(Control control)
    {
        control.PerformLayout();
        foreach (Control child in control.Controls) LayoutTree(child);
    }

    private static void CopyProperties(Control source, Control target)
    {
        target.Name = source.Name;
        target.Font = source.Font;
        target.Margin = source.Margin;
        target.Padding = source.Padding;
        target.Tag = source.Tag;
        target.AccessibleName = source.AccessibleName;
        target.AccessibleDescription = source.AccessibleDescription;
        target.TabIndex = source.TabIndex;
        target.TabStop = source.TabStop;
        target.Enabled = source.Enabled;
        target.Visible = source.Visible;
        target.Size = source.Size;
        // Some native inputs report their stock 23px height even when the
        // designer row is taller. Keep the themed minimum in those cases.
        target.MinimumSize = new Size(Math.Max(target.MinimumSize.Width, source.MinimumSize.Width), Math.Max(target.MinimumSize.Height, source.MinimumSize.Height));
        if (target is ModernChoice || target is ModernNumber || target is ModernSlider)
            target.Height = Math.Max(34, source.Height);
    }

    private Control Promote(Control original)
    {
        Control result;
        if (original is TableLayoutPanel table)
        {
            // The generated designer tables represent vertical stacks and
            // side-by-side columns. Read their *actual* cells, not a hardcoded
            // runtime copy of the design-time layout. Multiple edited rows are
            // also supported by grouping their cells into independent rows.
            var entries = table.Controls.Cast<Control>().Select(c => new { Control = c, Position = table.GetPositionFromControl(c) })
                .OrderBy(e => e.Position.Row).ThenBy(e => e.Position.Column).ToArray();
            var styles = table.ColumnStyles.Cast<ColumnStyle>().Select(s => (s.SizeType, s.Width)).ToArray();
            table.SuspendLayout();
            table.Controls.Clear();
            ContentGroup group;
            if (table.ColumnCount <= 1)
            {
                group = NewGroup(table, ContentDirection.Stack);
                foreach (var entry in entries) group.AddItem(ModernScrollHost.Wrap(Promote(entry.Control)));
            }
            else if (entries.Select(e => e.Position.Row).Distinct().Count() <= 1)
            {
                group = NewGroup(table, ContentDirection.Columns);
                group.DesignerColumns = entries.Select(e => ColumnAt(styles, e.Position.Column)).ToArray();
                foreach (var entry in entries) group.AddItem(ModernScrollHost.Wrap(Promote(entry.Control)));
            }
            else
            {
                group = NewGroup(table, ContentDirection.Stack);
                foreach (var row in entries.GroupBy(e => e.Position.Row))
                {
                    var cells = new ContentGroup(ContentDirection.Columns, designerSizing: true)
                    {
                        DesignerColumns = row.Select(e => ColumnAt(styles, e.Position.Column)).ToArray(), Tag = "inherit"
                    };
                    foreach (var entry in row) cells.AddItem(ModernScrollHost.Wrap(Promote(entry.Control)));
                    group.AddItem(cells);
                }
            }
            result = group;
        }
        else if (original is FlowLayoutPanel flow)
        {
            var children = flow.Controls.Cast<Control>().ToArray();
            flow.SuspendLayout();
            flow.Controls.Clear();
            var group = NewGroup(flow, flow.FlowDirection == FlowDirection.TopDown || flow.FlowDirection == FlowDirection.BottomUp ? ContentDirection.Stack : ContentDirection.Row);
            if (flow.FlowDirection == FlowDirection.RightToLeft || flow.FlowDirection == FlowDirection.BottomUp) Array.Reverse(children);
            foreach (var child in children) group.AddItem(ModernScrollHost.Wrap(Promote(child)));
            result = group;
        }
        else if (original is ComboBox choice)
        {
            var themed = new ModernChoice();
            themed.Items.AddRange(choice.Items.Cast<object>());
            themed.SelectedIndex = choice.SelectedIndex;
            CopyProperties(choice, themed);
            result = themed;
        }
        else if (original is NumericUpDown number)
        {
            var themed = new ModernNumber(decimal.ToInt32(number.Value), decimal.ToInt32(number.Minimum), decimal.ToInt32(number.Maximum));
            CopyProperties(number, themed);
            if (!string.IsNullOrWhiteSpace(number.AccessibleName)) themed.SetLabel(number.AccessibleName);
            result = themed;
        }
        else if (original is TrackBar slider)
        {
            var themed = new ModernSlider(slider.Minimum, slider.Maximum) { Value = slider.Value };
            CopyProperties(slider, themed);
            result = themed;
        }
        else if (original is CheckBox check)
        {
            var themed = new ModernCheckBox { Text = check.Text, CheckState = check.CheckState, ThreeState = check.ThreeState, UseMnemonic = check.UseMnemonic, AutoCheck = check.AutoCheck };
            CopyProperties(check, themed);
            result = themed;
        }
        else if (original is CheckedListBox checkedList)
        {
            var themed = new ModernCheckedListBox { CheckOnClick = checkedList.CheckOnClick, HorizontalScrollbar = checkedList.HorizontalScrollbar, BorderStyle = checkedList.BorderStyle, IntegralHeight = false };
            for (int i = 0; i < checkedList.Items.Count; i++) themed.Items.Add(checkedList.Items[i], checkedList.GetItemCheckState(i));
            CopyProperties(checkedList, themed);
            themed.RefreshExtent();
            result = themed;
        }
        else if (original is ListBox list)
        {
            var themed = new ModernListBox { SelectionMode = list.SelectionMode, HorizontalScrollbar = list.HorizontalScrollbar, AllowDrop = list.AllowDrop, BorderStyle = list.BorderStyle };
            themed.Items.AddRange(list.Items.Cast<object>().ToArray());
            CopyProperties(list, themed);
            themed.RefreshExtent();
            result = themed;
        }
        else if (original is Button button)
        {
            // Keep the stock Button instance and its designer event handlers.
            // A paint adapter supplies the same rounded primary/action style.
            _ = new ShellButtonStyle(button, (button.Tag as string) == "primary" ? ShellButtonKind.Primary : ShellButtonKind.Action, "");
            result = button;
        }
        else
        {
            result = original;
            if (result is DataGridView grid)
            {
                grid.EditingControlShowing += (_, e) => Theme.Apply(e.Control);
                typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(grid, true, null);
            }
            else if (result.Controls.Count > 0)
            {
                // Preserve ordinary designer panels (including the swatch).
                // Theme any additional controls placed in them by the editor.
                foreach (var child in result.Controls.Cast<Control>().ToArray())
                {
                    var bounds = child.Bounds; var anchor = child.Anchor; var dock = child.Dock;
                    result.Controls.Remove(child);
                    var item = ModernScrollHost.Wrap(Promote(child));
                    result.Controls.Add(item); item.Bounds = bounds; item.Anchor = anchor; item.Dock = dock;
                }
            }
        }
        if (!string.IsNullOrEmpty(result.Name)) controls.Add(result.Name, result);
        if (!ReferenceEquals(original, result)) original.Dispose();
        return result;
    }

    private static (SizeType Type, float Width) ColumnAt((SizeType SizeType, float Width)[] styles, int index) =>
        index >= 0 && index < styles.Length ? styles[index] : (SizeType.Percent, 100f);

    private static ContentGroup NewGroup(Control source, ContentDirection direction)
    {
        var group = new ContentGroup(direction, designerSizing: true);
        CopyProperties(source, group);
        if ((group.Tag as string) == "surface")
        {
            group.Paint += (_, e) =>
            {
                if (group.Width < 2 || group.Height < 2) return;
                using var pen = new Pen(Theme.Border);
                using var path = Ui.Round(new Rectangle(0, 0, group.Width - 1, group.Height - 1), 8);
                e.Graphics.DrawPath(pen, path);
            };
        }
        return group;
    }

    internal void BindClick(string buttonName, Action action)
    {
        var button = Get<Button>(buttonName);
        button.Click += (_, _) => { try { action(); } catch (Exception ex) { Ui.Error(button, ex); } };
    }
    internal void BindAsync(string buttonName, Func<Task> action)
    {
        var button = Get<Button>(buttonName);
        button.Click += async (_, _) => { try { await action(); } catch (Exception ex) { Ui.Error(button, ex); } };
    }
    internal void BindPath(string pathName, string buttonName, string label, bool save, string filter, bool folder)
    {
        var path = Get<TextBox>(pathName);
        BindClick(buttonName, () =>
        {
            if (folder)
            {
                using var dialog = new FolderBrowserDialog { Description = label, ShowNewFolderButton = false };
                if (dialog.ShowDialog(path.FindForm()) == DialogResult.OK) path.Text = dialog.SelectedPath;
            }
            else if (save)
            {
                using var dialog = new SaveFileDialog { Filter = filter, OverwritePrompt = false, AddExtension = true };
                if (dialog.ShowDialog(path.FindForm()) == DialogResult.OK) path.Text = dialog.FileName;
            }
            else
            {
                using var dialog = new OpenFileDialog { Filter = filter, CheckFileExists = true };
                if (dialog.ShowDialog(path.FindForm()) == DialogResult.OK) path.Text = dialog.FileName;
            }
        });
        path.AllowDrop = !save;
        path.DragEnter += (_, e) => { if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true) e.Effect = DragDropEffects.Copy; };
        path.DragDrop += (_, e) => { if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0) path.Text = files[0]; };
    }
    internal void BindFiles(string listName, Action changed, params string[] buttons)
    {
        var list = Get<ModernListBox>(listName);
        void Add(IEnumerable<string> files) { Ui.AddFiles(list, files); changed(); }
        list.DragEnter += (_, e) => { if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true) e.Effect = DragDropEffects.Copy; };
        list.DragDrop += (_, e) => { if (e.Data?.GetData(DataFormats.FileDrop) is string[] files) Add(files); };
        BindClick(buttons[0], () => { using var d = new OpenFileDialog { Multiselect = true, Filter = Ui.All }; if (d.ShowDialog(list.FindForm()) == DialogResult.OK) Add(d.FileNames); });
        BindClick(buttons[1], () => { foreach (var item in list.SelectedItems.Cast<object>().ToArray()) list.Items.Remove(item); list.RefreshExtent(); changed(); });
        BindClick(buttons[2], () => { list.Items.Clear(); list.RefreshExtent(); changed(); });
        void Move(int step)
        {
            if (list.SelectedItems.Count != 1) return;
            int i = list.SelectedIndex, j = i + step;
            if (j < 0 || j >= list.Items.Count) return;
            var item = list.Items[i]; list.Items.RemoveAt(i); list.Items.Insert(j, item); list.SelectedIndex = j; changed();
        }
        if (buttons.Length == 5) { BindClick(buttons[3], () => Move(-1)); BindClick(buttons[4], () => Move(1)); }
    }
}
