using System.Drawing.Drawing2D;
using System.Globalization;

namespace RTools.UI;

// All tool-page controls are runtime-only. The main form designer still uses
// ordinary WinForms controls; the editor frames preserve the native EDIT HWND.
internal static class InputDrawing
{
    internal static int Px(Control c, int n) => Math.Max(1, (int)Math.Round(n * c.DeviceDpi / 96.0));

    internal static void Frame(Control c, Graphics g, Color fill, bool focused)
    {
        g.Clear(c.Parent?.BackColor ?? Theme.Bg);
        if (c.Width < 4 || c.Height < 4) return;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = Ui.Round(new Rectangle(1, 1, c.Width - 3, c.Height - 3), Px(c, 6));
        using var brush = new SolidBrush(fill);
        using var pen = new Pen(focused && c.Enabled ? Theme.Accent : Theme.Border, focused && c.Enabled ? 2 : 1);
        g.FillPath(brush, path);
        g.DrawPath(pen, path);
    }

    internal static void Check(Graphics g, Rectangle bounds, CheckState state, bool enabled, bool hot)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = Ui.Round(bounds, Math.Max(2, bounds.Width / 4));
        bool marked = state != CheckState.Unchecked;
        Color fill = !enabled ? Theme.Input : marked ? Theme.Primary : hot ? Theme.Highlight : Theme.Input;
        using var brush = new SolidBrush(fill);
        using var pen = new Pen(!enabled ? Theme.Border : marked || hot ? Theme.Accent : Theme.Muted);
        g.FillPath(brush, path); g.DrawPath(pen, path);
        if (!marked) return;
        using var tick = new Pen(enabled ? Color.White : Theme.Muted, Math.Max(1.4f, bounds.Width / 8f))
            { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        if (state == CheckState.Indeterminate)
            g.DrawLine(tick, bounds.Left + bounds.Width * .25f, bounds.Top + bounds.Height * .5f, bounds.Left + bounds.Width * .75f, bounds.Top + bounds.Height * .5f);
        else g.DrawLines(tick, new[] {
            new PointF(bounds.Left + bounds.Width * .23f, bounds.Top + bounds.Height * .52f),
            new PointF(bounds.Left + bounds.Width * .43f, bounds.Top + bounds.Height * .72f),
            new PointF(bounds.Left + bounds.Width * .78f, bounds.Top + bounds.Height * .30f) });
    }

    internal static void Chevron(Graphics g, Rectangle r, bool up, Color color)
    {
        float x = r.Left + r.Width / 2f, y = r.Top + r.Height / 2f;
        float d = Math.Max(2f, Math.Min(r.Width, r.Height) * .22f);
        using var pen = new Pen(color, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        float sign = up ? 1f : -1f;
        g.DrawLines(pen, new[] { new PointF(x-d, y+sign*d*.45f), new PointF(x, y-sign*d*.45f), new PointF(x+d, y+sign*d*.45f) });
    }
}

internal sealed class ModernInputFrame : Panel
{
    private readonly TextBox editor;
    internal ModernInputFrame(TextBox editor)
    {
        this.editor = editor;
        Size = new Size(editor.Width, Math.Max(editor.Height, 34));
        Margin = editor.Margin; TabIndex = editor.TabIndex; TabStop = false; Tag = "inherit";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        editor.BorderStyle = BorderStyle.None; editor.Dock = DockStyle.None; editor.Anchor = AnchorStyles.Top | AnchorStyles.Left; editor.Margin = Padding.Empty;
        Controls.Add(editor);
        editor.Enter += (_, _) => Invalidate(); editor.Leave += (_, _) => Invalidate();
        editor.FontChanged += (_, _) => PerformLayout();
    }
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        if (editor == null) return;
        int pad = InputDrawing.Px(this, 9), h = editor.PreferredHeight;
        var bounds = new Rectangle(pad, Math.Max(2, (Height-h)/2), Math.Max(1, Width-pad*2), h);
        if (editor.Bounds != bounds) editor.Bounds = bounds;
    }
    protected override void OnPaint(PaintEventArgs e) => InputDrawing.Frame(this, e.Graphics, Theme.Input, editor.Focused);
    protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); if (e.Button == MouseButtons.Left) editor.Focus(); }
}

// A native editable text field with a themed spinner; integer values are the
// only numeric input used by the toolbox. Keep the decimal Value API so the
// existing tool calculations and synchronization need no changes.
internal sealed class ModernNumber : UserControl
{
    private readonly TextBox editor;
    private readonly System.Windows.Forms.Timer repeat;
    private decimal value;
    private bool syncing;
    private int hot, pressed;
    internal decimal Minimum { get; }
    internal decimal Maximum { get; }
    internal event EventHandler? ValueChanged;
    internal decimal Value
    {
        get => value;
        set => SetValue(value, true);
    }

    internal ModernNumber(int initial, int min, int max)
    {
        if (min > max) throw new ArgumentOutOfRangeException(nameof(min));
        Minimum = min; Maximum = max;
        Size = new Size(120, 34); MinimumSize = new Size(84, 30);
        Margin = new Padding(0, 0, 0, 8); TabStop = false;
        AccessibleRole = AccessibleRole.SpinButton;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        editor = new NumberEdit { BorderStyle = BorderStyle.None, Font = Font, TabStop = true, TextAlign = HorizontalAlignment.Left, AccessibleName = "Number" };
        repeat = new System.Windows.Forms.Timer { Interval = 400 };
        repeat.Tick += (_, _) => { if (pressed == 0 || !Capture || !Enabled) StopRepeat(); else { Step(pressed); repeat.Interval = 75; } };
        Controls.Add(editor);
        editor.TextChanged += (_, _) =>
        {
            if (!syncing && TryRead(out decimal n) && n >= Minimum && n <= Maximum) SetValue(n, false);
        };
        editor.Enter += (_, _) => Invalidate();
        editor.Leave += (_, _) => { CommitText(); Invalidate(); };
        editor.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            { Step(e.KeyCode == Keys.Up ? 1 : -1); e.Handled = e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
            { if (e.KeyCode == Keys.Enter) CommitText(); else UpdateText(); e.Handled = e.SuppressKeyPress = true; }
        };
        editor.MouseWheel += (_, e) => { if (editor.Focused && e.Delta != 0) { Step(Math.Sign(e.Delta)); if (e is HandledMouseEventArgs handled) handled.Handled = true; } };
        SetValue(initial, true);
    }

    private sealed class NumberEdit : TextBox
    {
        protected override bool IsInputKey(Keys keyData)
        {
            var key = keyData & Keys.KeyCode;
            return key == Keys.Up || key == Keys.Down || key == Keys.Enter || key == Keys.Escape || base.IsInputKey(keyData);
        }
    }
    private bool TryRead(out decimal n) => decimal.TryParse(editor.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out n);
    private void SetValue(decimal n, bool normalize)
    {
        n = Math.Max(Minimum, Math.Min(Maximum, decimal.Truncate(n)));
        bool changed = n != value; value = n;
        // During ordinary typing do not move the caret or rewrite the text.
        if (normalize && (changed || !TryRead(out decimal parsed) || parsed != n)) UpdateText();
        Invalidate();
        if (changed) { if (IsHandleCreated) AccessibilityNotifyClients(AccessibleEvents.ValueChange, -1); ValueChanged?.Invoke(this, EventArgs.Empty); }
    }
    private void UpdateText()
    {
        syncing = true;
        try { editor.Text = value.ToString("0", CultureInfo.CurrentCulture); }
        finally { syncing = false; }
    }
    private void CommitText() { if (TryRead(out decimal n)) SetValue(n, true); UpdateText(); }
    private void Step(int direction) { CommitText(); SetValue(value + direction, true); }
    internal void SetLabel(string label) { AccessibleName = label; editor.AccessibleName = label; }
    private int ArrowWidth => InputDrawing.Px(this, 28);
    private Rectangle Up => new Rectangle(Math.Max(3, Width-ArrowWidth-3), 3, ArrowWidth, Math.Max(1, (Height-6)/2));
    private Rectangle Down => new Rectangle(Up.X, Up.Bottom, Up.Width, Math.Max(1, Height-3-Up.Bottom));
    private int Hit(Point p) => Up.Contains(p) ? 1 : Down.Contains(p) ? -1 : 0;
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        if (editor == null) return;
        int pad = InputDrawing.Px(this, 9), h = editor.PreferredHeight;
        var bounds = new Rectangle(pad, Math.Max(2,(Height-h)/2), Math.Max(1, Width-ArrowWidth-pad-6), h);
        if (editor.Bounds != bounds) editor.Bounds = bounds;
    }
    protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); if (editor != null) { editor.Font = Font; PerformLayout(); } }
    protected override void OnPaint(PaintEventArgs e)
    {
        InputDrawing.Frame(this, e.Graphics, Theme.Input, editor.Focused || pressed != 0);
        if (Width < 8 || Height < 8) return;
        using (var border = new Pen(Theme.Border)) e.Graphics.DrawLine(border, Up.Left-2, Up.Top+2, Up.Left-2, Down.Bottom-2);
        foreach (int direction in new[] { 1, -1 })
        {
            Rectangle r = direction == 1 ? Up : Down;
            bool available = Enabled && (direction == 1 ? value < Maximum : value > Minimum);
            if (available && (hot == direction || pressed == direction))
            {
                using var shape = Ui.Round(r, 4); using var fill = new SolidBrush(pressed == direction ? Theme.Highlight : Theme.Surface);
                e.Graphics.FillPath(fill, shape);
            }
            InputDrawing.Chevron(e.Graphics, r, direction == 1, !available ? Theme.Border : hot == direction || pressed == direction ? Theme.SelectedText : Theme.Ink);
        }
    }
    protected override void OnMouseMove(MouseEventArgs e) { base.OnMouseMove(e); int next = Hit(e.Location); if (next != hot) { hot = next; Invalidate(); } }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hot = 0; Invalidate(); }
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e); if (e.Button != MouseButtons.Left || !Enabled) return;
        editor.Focus(); pressed = Hit(e.Location); if (pressed == 0) return;
        Capture = true; Step(pressed); repeat.Interval = 400; repeat.Start(); Invalidate();
    }
    private void StopRepeat() { repeat.Stop(); pressed = 0; Invalidate(); }
    protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); StopRepeat(); Capture = false; }
    protected override void OnMouseCaptureChanged(EventArgs e) { base.OnMouseCaptureChanged(e); if (repeat != null && !Capture) StopRepeat(); }
    protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); if (!Enabled && repeat != null) { StopRepeat(); Capture = false; } Invalidate(); }
    protected override void OnVisibleChanged(EventArgs e) { base.OnVisibleChanged(e); if (!Visible && repeat != null) { StopRepeat(); Capture = false; } }
    protected override void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed && repeat != null) { StopRepeat(); Capture = false; }
        base.Dispose(disposing);
        if (disposing) repeat?.Dispose();
    }
}

internal sealed class ModernSlider : Control
{
    private int value;
    private bool hot, dragging;
    internal int Minimum { get; }
    internal int Maximum { get; }
    internal event EventHandler? ValueChanged;
    internal int Value
    {
        get => value;
        set
        {
            int n = Math.Max(Minimum, Math.Min(Maximum, value));
            if (n == this.value) return;
            this.value = n; Invalidate();
            if (IsHandleCreated) AccessibilityNotifyClients(AccessibleEvents.ValueChange, -1);
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    internal ModernSlider(int minimum = 0, int maximum = 255)
    {
        if (maximum <= minimum) throw new ArgumentOutOfRangeException(nameof(maximum));
        Minimum = minimum; Maximum = maximum; value = minimum;
        Size = new Size(180, 34); MinimumSize = new Size(50, 30); Margin = new Padding(0, 0, 0, 6);
        Cursor = Cursors.Hand; TabStop = true; AccessibleRole = AccessibleRole.Slider; Tag = "inherit";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
    }
    private int Pad => InputDrawing.Px(this, 13);
    private int Span => Math.Max(1, Width-Pad*2);
    private int Thumb => Pad + (int)Math.Round((Value-(double)Minimum)/(Maximum-(double)Minimum)*Span);
    private void SetAt(int x) => Value = Minimum + (int)Math.Round(Math.Max(0d, Math.Min(1d, (x-Pad)/(double)Span))*(Maximum-(double)Minimum));
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? Theme.Bg); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        if (Width <= Pad*2 || Height < 6) return;
        int y = Height/2, thumb = Thumb;
        using (var pen = new Pen(Theme.Border, InputDrawing.Px(this, 4)) { StartCap = LineCap.Round, EndCap = LineCap.Round }) e.Graphics.DrawLine(pen, Pad, y, Width-Pad, y);
        if (thumb > Pad) using (var pen = new Pen(Enabled ? Theme.Accent : Theme.Muted, InputDrawing.Px(this, 4)) { StartCap = LineCap.Round, EndCap = LineCap.Round }) e.Graphics.DrawLine(pen, Pad, y, thumb, y);
        int radius = InputDrawing.Px(this, dragging ? 10 : hot ? 10 : 9);
        using (var outer = new SolidBrush(Enabled ? Theme.Input : Theme.Surface)) e.Graphics.FillEllipse(outer, thumb-radius, y-radius, radius*2, radius*2);
        using (var border = new Pen(Focused || dragging ? Theme.Accent : Theme.Muted, Focused ? 2 : 1)) e.Graphics.DrawEllipse(border, thumb-radius, y-radius, radius*2, radius*2);
        int inner = InputDrawing.Px(this, dragging ? 4 : 5);
        using var dot = new SolidBrush(Enabled ? Theme.Accent : Theme.Muted); e.Graphics.FillEllipse(dot, thumb-inner, y-inner, inner*2, inner*2);
    }
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hot = true; Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hot = false; Invalidate(); }
    protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); if (e.Button == MouseButtons.Left && Enabled) { Focus(); dragging = true; Capture = true; SetAt(e.X); Invalidate(); } }
    protected override void OnMouseMove(MouseEventArgs e) { base.OnMouseMove(e); if (dragging && Capture) SetAt(e.X); }
    protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); if (dragging) SetAt(e.X); dragging = false; Capture = false; Invalidate(); }
    protected override void OnMouseCaptureChanged(EventArgs e) { base.OnMouseCaptureChanged(e); if (!Capture) { dragging = false; Invalidate(); } }
    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
    protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); if (!Enabled) { dragging = false; Capture = false; } Invalidate(); }
    protected override void OnMouseWheel(MouseEventArgs e) { base.OnMouseWheel(e); if (Focused && Enabled && e.Delta != 0) { Value += Math.Sign(e.Delta); if (e is HandledMouseEventArgs h) h.Handled = true; } }
    protected override bool IsInputKey(Keys keyData)
    {
        var key = keyData & Keys.KeyCode;
        return key == Keys.Left || key == Keys.Right || key == Keys.Up || key == Keys.Down || key == Keys.Home || key == Keys.End || key == Keys.PageUp || key == Keys.PageDown || base.IsInputKey(keyData);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.KeyCode)
        {
            case Keys.Left: case Keys.Down: Value--; break;
            case Keys.Right: case Keys.Up: Value++; break;
            case Keys.Home: Value = Minimum; break;
            case Keys.End: Value = Maximum; break;
            case Keys.PageUp: Value += 10; break;
            case Keys.PageDown: Value -= 10; break;
            default: return;
        }
        e.Handled = e.SuppressKeyPress = true;
    }
    protected override AccessibleObject CreateAccessibilityInstance() => new SliderAccessible(this);
    private sealed class SliderAccessible : Control.ControlAccessibleObject
    {
        private readonly ModernSlider slider;
        internal SliderAccessible(ModernSlider slider) : base(slider) { this.slider = slider; }
        public override string Value { get => slider.Value.ToString(CultureInfo.CurrentCulture); set { if (int.TryParse(value, out int n)) slider.Value = n; } }
        public override AccessibleRole Role => AccessibleRole.Slider;
    }
}

internal sealed class ModernCheckBox : CheckBox
{
    private bool hot;
    internal ModernCheckBox()
    {
        FlatStyle = FlatStyle.Flat; UseVisualStyleBackColor = false; Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }
    public override Size GetPreferredSize(Size proposedSize)
    {
        var text = TextRenderer.MeasureText(Text, Font, Size.Empty, TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
        return new Size(text.Width + InputDrawing.Px(this, 28), Math.Max(text.Height + 6, InputDrawing.Px(this, 25)));
    }
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hot = true; Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hot = false; Invalidate(); }
    protected override void OnCheckedChanged(EventArgs e) { base.OnCheckedChanged(e); Invalidate(); }
    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? Theme.Bg);
        int side = Math.Max(4, Math.Min(InputDrawing.Px(this, 17), Height-4));
        var check = new Rectangle(2, Math.Max(1,(Height-side)/2), side, side);
        InputDrawing.Check(e.Graphics, check, CheckState, Enabled, hot);
        int left = check.Right + InputDrawing.Px(this, 7);
        var text = new Rectangle(left,0,Math.Max(1,Width-left),Height);
        TextRenderer.DrawText(e.Graphics,Text,Font,text,Enabled ? Theme.Ink : Theme.Muted,TextFormatFlags.NoPrefix|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.EndEllipsis);
        if (Focused && ShowFocusCues && Width > 5) { using var pen = new Pen(Theme.Accent); using var path = Ui.Round(new Rectangle(0,0,Width-1,Height-1),4); e.Graphics.DrawPath(pen,path); }
    }
}

internal static class ListDrawing
{
    internal static void Item(ListBox list, DrawItemEventArgs e, CheckState? check = null)
    {
        bool selected = (e.State & DrawItemState.Selected) != 0;
        using (var bg = new SolidBrush(selected ? Theme.Highlight : Theme.Input)) e.Graphics.FillRectangle(bg, e.Bounds);
        if (e.Index < 0 || e.Index >= list.Items.Count) return;
        int left = e.Bounds.Left + 6;
        if (check.HasValue)
        {
            int side = Math.Max(4,Math.Min(15,e.Bounds.Height-3));
            InputDrawing.Check(e.Graphics, new Rectangle(left,e.Bounds.Top+(e.Bounds.Height-side)/2,side,side), check.Value, list.Enabled, false);
            left += side+7;
        }
        TextRenderer.DrawText(e.Graphics, list.GetItemText(list.Items[e.Index]), list.Font,
            new Rectangle(left,e.Bounds.Top,Math.Max(1,e.Bounds.Right-left),e.Bounds.Height), list.Enabled ? (selected ? Theme.SelectedText : Theme.Ink) : Theme.Muted,
            TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        if (selected && (e.State & DrawItemState.Focus) != 0) { using var pen = new Pen(Theme.Accent); e.Graphics.DrawRectangle(pen,e.Bounds.Left,e.Bounds.Top,Math.Max(1,e.Bounds.Width-1),Math.Max(1,e.Bounds.Height-1)); }
    }
}
internal sealed class ModernListBox : ListBox
{
    internal ModernListBox() { DrawMode = DrawMode.OwnerDrawFixed; ItemHeight = 25; IntegralHeight = false; }
    protected override void OnDrawItem(DrawItemEventArgs e) => ListDrawing.Item(this,e);
    internal void RefreshExtent() => HorizontalExtent = Items.Cast<object>().Select(i => TextRenderer.MeasureText(GetItemText(i),Font).Width+18).DefaultIfEmpty(0).Max();
}
internal sealed class ModernCheckedListBox : CheckedListBox
{
    // CheckedListBox already uses an owner-drawn item window. Retain its native
    // selection, accessibility and check toggling; replace only item rendering.
    protected override void OnDrawItem(DrawItemEventArgs e) => ListDrawing.Item(this,e,e.Index >= 0 && e.Index < Items.Count ? GetItemCheckState(e.Index) : (CheckState?)null);
    internal void RefreshExtent() => HorizontalExtent = Items.Cast<object>().Select(i => TextRenderer.MeasureText(GetItemText(i),Font).Width+38).DefaultIfEmpty(0).Max();
}
