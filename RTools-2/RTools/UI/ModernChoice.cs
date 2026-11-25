using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;

namespace RTools.UI;

// A selection-only combo control. The application uses no editable combo boxes.
// The hosted native ListBox preserves item navigation, incremental search and
// screen-reader list semantics; the popup and its rail are styled by RTools.
internal sealed class ModernChoice : Control
{
    internal sealed class ChoiceItems : Collection<object>
    {
        private readonly ModernChoice owner;
        internal ChoiceItems(ModernChoice owner) { this.owner = owner; }
        internal void AddRange(IEnumerable<object> items) { foreach (var item in items) Add(item); }
        protected override void InsertItem(int index, object item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            base.InsertItem(index, item);
            owner.ItemsChanged(owner.selectedIndex >= index ? owner.selectedIndex + 1 : owner.selectedIndex);
        }
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            owner.ItemsChanged(owner.selectedIndex == index ? -1 : owner.selectedIndex > index ? owner.selectedIndex - 1 : owner.selectedIndex);
        }
        protected override void ClearItems() { base.ClearItems(); owner.ItemsChanged(-1); }
        protected override void SetItem(int index, object item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            base.SetItem(index, item); owner.ItemsChanged(owner.selectedIndex, owner.selectedIndex == index);
        }
    }

    private int selectedIndex = -1;
    private bool hot;
    private ToolStripDropDown? popup;
    private int lastCloseTick;
    private bool suppressReopen;
    internal ChoiceItems Items { get; }
    internal event EventHandler? SelectedIndexChanged;
    internal int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (value < -1 || value >= Items.Count) throw new ArgumentOutOfRangeException(nameof(value));
            ChangeSelection(value);
        }
    }
    internal object? SelectedItem => selectedIndex < 0 ? null : Items[selectedIndex];
    internal bool DroppedDown => popup != null && popup.Visible;

    internal ModernChoice()
    {
        Items = new ChoiceItems(this);
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.StandardClick, true);
        Size = new Size(180, 34); MinimumSize = new Size(60, 30);
        Font = new Font("Segoe UI", 10);
        Cursor = Cursors.Hand; TabStop = true;
        AccessibleRole = AccessibleRole.ComboBox;
        Margin = new Padding(0, 0, 0, 8);
    }
    private int Px(int n) => Math.Max(1, (int)Math.Round(n * DeviceDpi / 96.0));
    private void ChangeSelection(int value, bool force = false)
    {
        if (selectedIndex == value && !force) { Invalidate(); return; }
        selectedIndex = value;
        Invalidate();
        if (IsHandleCreated) AccessibilityNotifyClients(AccessibleEvents.ValueChange, -1);
        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
    }
    private void ItemsChanged(int index, bool force = false)
    {
        ClosePopup();
        ChangeSelection(index, force);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? Theme.Bg);
        if (Width < 4 || Height < 4) return;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = new Rectangle(1, 1, Width - 3, Height - 3);
        using var shape = Ui.Round(bounds, Px(6));
        using var bg = new SolidBrush(!Enabled ? Theme.Input : hot || DroppedDown ? Theme.Highlight : Theme.Input);
        using var border = new Pen((Focused || DroppedDown) && Enabled ? Theme.Accent : Theme.Border, Focused || DroppedDown ? Px(2) : 1);
        e.Graphics.FillPath(bg, shape); e.Graphics.DrawPath(border, shape);
        Color fg = Enabled ? Theme.Ink : Theme.Muted;
        int arrowWidth = Px(34), left = Px(11), mid = Height / 2;
        var textRect = new Rectangle(left, 0, Math.Max(1, Width - arrowWidth - left), Height);
        TextRenderer.DrawText(e.Graphics, SelectedItem?.ToString() ?? "", Font, textRect, fg,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
        int x = Width - arrowWidth / 2, d = Px(4);
        using var pen = new Pen(Enabled ? Theme.Muted : Theme.Border, Math.Max(1, Px(2))) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        if (DroppedDown) e.Graphics.DrawLines(pen, new[] { new Point(x - d, mid + d / 2), new Point(x, mid - d / 2), new Point(x + d, mid + d / 2) });
        else e.Graphics.DrawLines(pen, new[] { new Point(x - d, mid - d / 2), new Point(x, mid + d / 2), new Point(x + d, mid - d / 2) });
    }
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hot = true; Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hot = false; Invalidate(); }
    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
    protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); if (!Enabled) ClosePopup(); Invalidate(); }
    protected override void OnVisibleChanged(EventArgs e) { base.OnVisibleChanged(e); if (!Visible) ClosePopup(); }
    protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); if (e.Button == MouseButtons.Left) Focus(); }
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button != MouseButtons.Left) return;
        // ToolStripDropDown may dismiss itself before the owner's mouse event.
        // Do not immediately reopen when the click was intended to close it.
        if (suppressReopen && unchecked(Environment.TickCount - lastCloseTick) < 300)
        { suppressReopen = false; return; }
        suppressReopen = false;
        TogglePopup();
    }
    private void TogglePopup() { if (DroppedDown) ClosePopup(); else OpenPopup(); }
    private void SelectRelative(int step)
    {
        if (Items.Count == 0) return;
        SelectedIndex = Math.Max(0, Math.Min(Items.Count - 1, selectedIndex + step));
    }
    protected override bool IsInputKey(Keys keyData)
    {
        switch (keyData & Keys.KeyCode)
        { case Keys.Up: case Keys.Down: case Keys.Home: case Keys.End: case Keys.PageUp: case Keys.PageDown: return true; }
        return base.IsInputKey(keyData);
    }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F4 || keyData == (Keys.Alt | Keys.Down) || keyData == (Keys.Alt | Keys.Up) || keyData == Keys.Space || keyData == Keys.Enter)
        { TogglePopup(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Up: SelectRelative(-1); break;
            case Keys.Down: SelectRelative(1); break;
            case Keys.PageUp: SelectRelative(-8); break;
            case Keys.PageDown: SelectRelative(8); break;
            case Keys.Home: if (Items.Count != 0) SelectedIndex = 0; break;
            case Keys.End: if (Items.Count != 0) SelectedIndex = Items.Count - 1; break;
            default: base.OnKeyDown(e); return;
        }
        e.Handled = true; e.SuppressKeyPress = true;
    }
    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
        if (char.IsControl(e.KeyChar) || Items.Count == 0) return;
        for (int i = 1; i <= Items.Count; i++)
        {
            int index = (selectedIndex + i) % Items.Count;
            if ((Items[index].ToString() ?? "").StartsWith(e.KeyChar.ToString(), StringComparison.CurrentCultureIgnoreCase))
            { SelectedIndex = index; e.Handled = true; return; }
        }
    }
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (!Focused || DroppedDown) { base.OnMouseWheel(e); return; }
        if (e is HandledMouseEventArgs handled) handled.Handled = true;
        if (e.Delta != 0) SelectRelative(e.Delta > 0 ? -1 : 1);
    }

    internal void OpenPopup()
    {
        if (!Enabled || !Visible || !IsHandleCreated || DroppedDown || Items.Count == 0) return;
        popup?.Dispose();
        int rowHeight = Math.Max(Px(31), Font.Height + Px(9));
        int border = Px(5);
        var screen = Screen.FromControl(this).WorkingArea;
        var anchor = RectangleToScreen(ClientRectangle);
        int available = Math.Max(screen.Bottom - anchor.Bottom, anchor.Top - screen.Top);
        int rows = Math.Min(Math.Min(Items.Count, 8), Math.Max(1, (available - border * 2 - Px(4)) / rowHeight));
        int width = Math.Min(screen.Width - border * 2, Math.Max(Width - border * 2, Px(160)));
        // Size for long labels, with a bounded measuring pass; longer values
        // still have a native horizontal scroll range in the option list.
        foreach (var item in Items.Take(300))
            width = Math.Max(width, Math.Min(screen.Width - border * 2, TextRenderer.MeasureText(item.ToString() ?? "", Font).Width + Px(44)));
        int height = rows * rowHeight;
        var list = new PopupList { Font = Font, ItemHeight = rowHeight, DrawMode = DrawMode.OwnerDrawFixed,
            IntegralHeight = false, BorderStyle = BorderStyle.None, HorizontalScrollbar = true,
            BackColor = Theme.Surface, ForeColor = Theme.Ink, Size = new Size(width, height), Margin = Padding.Empty,
            AccessibleName = (AccessibleName ?? "Options") + " options" };
        list.Items.AddRange(Items.ToArray());
        list.HorizontalExtent = Math.Max(0, Items.Take(300).Select(item => TextRenderer.MeasureText(item.ToString() ?? "", Font).Width + Px(22)).DefaultIfEmpty(0).Max());
        list.SelectedIndex = selectedIndex;
        if (list.SelectedIndex >= 0) list.TopIndex = Math.Max(0, list.SelectedIndex - rows + 1);
        var host = new ModernScrollHost(list) { Dock = DockStyle.None, Size = new Size(width, height), BackColor = Theme.Surface, Margin = Padding.Empty };
        // Set size explicitly: ToolStripControlHost otherwise adopts its child's preferred size.
        var itemHost = new ToolStripControlHost(host) { AutoSize = false, Size = host.Size, Margin = Padding.Empty, Padding = Padding.Empty };
        var drop = new RoundedPopup { Padding = new Padding(border), Margin = Padding.Empty, AutoSize = true, BackColor = Theme.Surface, Renderer = new PopupRenderer(), DropShadowEnabled = true };
        popup = drop;
        drop.Items.Add(itemHost);
        void Commit()
        {
            int index = list.SelectedIndex;
            if (index < 0 || index >= Items.Count) return;
            drop.Close(ToolStripDropDownCloseReason.ItemClicked);
            if (!IsDisposed) { Focus(); SelectedIndex = index; }
        }
        list.Commit = Commit;
        list.Cancel = () => { drop.Close(ToolStripDropDownCloseReason.Keyboard); if (!IsDisposed) Focus(); };
        list.TabAway = backwards =>
        {
            drop.Close(ToolStripDropDownCloseReason.Keyboard);
            if (!IsDisposed) { Focus(); FindForm()?.SelectNextControl(this, !backwards, true, true, true); }
        };
        list.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left && list.IndexFromPoint(e.Location) >= 0) Commit(); };
        drop.Closed += (_, e) =>
        {
            lastCloseTick = Environment.TickCount;
            suppressReopen = e.CloseReason == ToolStripDropDownCloseReason.AppClicked && RectangleToScreen(ClientRectangle).Contains(Cursor.Position);
            if (!IsDisposed)
            {
                Invalidate(); AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
                if (e.CloseReason == ToolStripDropDownCloseReason.Keyboard) Focus();
            }
        };
        var direction = screen.Bottom - anchor.Bottom >= height + border * 2 || screen.Bottom - anchor.Bottom >= anchor.Top - screen.Top
            ? ToolStripDropDownDirection.BelowRight : ToolStripDropDownDirection.AboveRight;
        drop.Show(this, new Point(0, direction == ToolStripDropDownDirection.BelowRight ? Height + Px(3) : -Px(3)), direction);
        if (list.SelectedIndex >= 0) list.TopIndex = Math.Max(0, list.SelectedIndex - rows + 1);
        list.Focus(); host.Synchronize();
        Invalidate(); AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
    }
    internal void ClosePopup() { if (popup != null && popup.Visible) popup.Close(); }
    protected override void Dispose(bool disposing)
    {
        if (disposing && popup != null) { var old = popup; popup = null; old.Dispose(); }
        base.Dispose(disposing);
    }

    protected override AccessibleObject CreateAccessibilityInstance() => new ChoiceAccessible(this);
    private sealed class ChoiceAccessible : ControlAccessibleObject
    {
        private readonly ModernChoice choice;
        internal ChoiceAccessible(ModernChoice choice) : base(choice) { this.choice = choice; }
        public override AccessibleRole Role => AccessibleRole.ComboBox;
        public override string? Value { get => choice.SelectedItem?.ToString(); set { } }
        public override string DefaultAction => choice.DroppedDown ? "Collapse" : "Expand";
        public override AccessibleStates State => base.State | (choice.DroppedDown ? AccessibleStates.Expanded : AccessibleStates.Collapsed);
        public override void DoDefaultAction() { if (choice.Enabled) choice.TogglePopup(); }
    }

    private sealed class PopupList : ListBox
    {
        private int hotIndex = -1;
        internal Action? Commit, Cancel;
        internal Action<bool>? TabAway;
        internal PopupList() { SelectionMode = SelectionMode.One; }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e); int index = IndexFromPoint(e.Location);
            if (hotIndex != index) { hotIndex = index; Invalidate(); }
        }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hotIndex = -1; Invalidate(); }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count) return;
            bool selected = (e.State & DrawItemState.Selected) != 0;
            using (var bg = new SolidBrush(Theme.Surface)) e.Graphics.FillRectangle(bg, e.Bounds);
            var row = Rectangle.Inflate(e.Bounds, -2, -2);
            if (row.Width <= 2 || row.Height <= 2) return;
            if (selected || e.Index == hotIndex)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var shape = Ui.Round(row, 5);
                using var brush = new SolidBrush(selected ? Theme.Highlight : Theme.Input);
                e.Graphics.FillPath(brush, shape);
            }
            if (selected)
            {
                using var accent = new Pen(Theme.Accent, 3) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                e.Graphics.DrawLine(accent, row.Left + 2, row.Top + 6, row.Left + 2, Math.Max(row.Top + 6, row.Bottom - 6));
            }
            var text = new Rectangle(row.Left + 10, row.Top, Math.Max(1, row.Width - 13), row.Height);
            TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font, text, Theme.Ink,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            if (selected && Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics, row, Theme.Ink, Theme.Highlight);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter || keyData == Keys.Space || keyData == Keys.F4 || keyData == (Keys.Alt | Keys.Up)) { Commit?.Invoke(); return true; }
            if (keyData == Keys.Escape) { Cancel?.Invoke(); return true; }
            if (keyData == Keys.Tab || keyData == (Keys.Shift | Keys.Tab)) { TabAway?.Invoke((keyData & Keys.Shift) != 0); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
    private sealed class RoundedPopup : ToolStripDropDown
    {
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (Width <= 1 || Height <= 1) return;
            using var path = Ui.Round(new Rectangle(0, 0, Width, Height), 8);
            var old = Region;
            Region = new Region(path);
            old?.Dispose();
        }
    }
    private sealed class PopupRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e) => e.Graphics.Clear(Theme.Surface);
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip.Width <= 3 || e.ToolStrip.Height <= 3) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var shape = Ui.Round(new Rectangle(1, 1, e.ToolStrip.Width - 3, e.ToolStrip.Height - 3), 7);
            using var pen = new Pen(Theme.Border); e.Graphics.DrawPath(pen, shape);
        }
    }
}
