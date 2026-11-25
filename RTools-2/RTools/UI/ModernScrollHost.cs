using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace RTools.UI;

// Runtime-only sibling overlays. Native controls keep their own scroll ranges,
// editing, selection, accessibility, mouse-wheel and keyboard implementations.
// The overlays never cover content; they occupy the native scrollbar rectangles.
internal sealed class ModernScrollHost : Panel
{
    private readonly Control viewport;
    private readonly Rail vertical, horizontal;
    private readonly Panel corner;
    private readonly System.Windows.Forms.Timer refreshTimer;
    private bool synchronizing;
    private readonly bool frameInput;

    internal ModernScrollHost(Control viewport)
    {
        this.viewport = viewport;
        Size = viewport.Size;
        MinimumSize = viewport.MinimumSize;
        Margin = viewport.Margin;
        Tag = viewport.Tag;
        TabIndex = viewport.TabIndex;
        Dock = DockStyle.Fill;
        TabStop = false;
        frameInput = (viewport is TextBoxBase text && text.BorderStyle != BorderStyle.None) ||
            (viewport is ListBox items && items.BorderStyle != BorderStyle.None) ||
            (viewport is DataGridView table && table.BorderStyle != BorderStyle.None);
        if (frameInput)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Padding = new Padding(4);
            if (viewport is TextBoxBase editor) editor.BorderStyle = BorderStyle.None;
            if (viewport is ListBox itemList) itemList.BorderStyle = BorderStyle.None;
            if (viewport is DataGridView data) data.BorderStyle = BorderStyle.None;
            viewport.Enter += (_, _) => Invalidate(); viewport.Leave += (_, _) => Invalidate();
        }
        // Fixed heights in tool cards must not change when the native list
        // rounds its own height down to a multiple of its item height.
        if (viewport is ListBox list) list.IntegralHeight = false;
        viewport.Margin = new Padding(0);
        viewport.Dock = DockStyle.Fill;
        vertical = new Rail(this, false) { Name = "modernVerticalScrollBar", Visible = false };
        horizontal = new Rail(this, true) { Name = "modernHorizontalScrollBar", Visible = false };
        corner = new Panel { Name = "modernScrollCorner", Visible = false, TabStop = false, Tag = "inherit" };
        Controls.Add(viewport);
        Controls.Add(vertical);
        Controls.Add(horizontal);
        Controls.Add(corner);
        corner.BringToFront(); horizontal.BringToFront(); vertical.BringToFront();
        refreshTimer = new System.Windows.Forms.Timer { Interval = 80 };
        refreshTimer.Tick += (_, _) => Synchronize();
        viewport.Layout += (_, _) => Synchronize();
        viewport.SizeChanged += (_, _) => Synchronize();
        viewport.MouseWheel += (_, _) => Synchronize();
        viewport.HandleCreated += (_, _) => Synchronize();
        if (viewport is ScrollableControl sc) sc.Scroll += (_, _) => Synchronize();
        if (viewport is DataGridView grid) grid.Scroll += (_, _) => Synchronize();
        HandleCreated += (_, _) => UpdateTimer();
        HandleDestroyed += (_, _) => { if (!Disposing && !IsDisposed) refreshTimer.Stop(); };
        VisibleChanged += (_, _) => UpdateTimer();
        SizeChanged += (_, _) => Synchronize();
    }

    internal static Control Wrap(Control control)
    {
        if (control is ModernScrollHost || control is ModernInputFrame) return control;
        if (control.Parent is ModernInputFrame frame) return frame;
        if (control.Parent is ModernScrollHost existing) return existing;
        if (control is TextBox box && box.Multiline || control is RichTextBox ||
            control is ListBox || control is DataGridView)
            return new ModernScrollHost(control);
        if (control is TextBox singleLine) return new ModernInputFrame(singleLine);
        return control;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (frameInput) InputDrawing.Frame(this, e.Graphics, TrackBackground, viewport.ContainsFocus);
    }

    private Color TrackBackground => viewport is DataGridView g ? g.BackgroundColor : viewport.BackColor;
    private void UpdateTimer()
    {
        if (IsDisposed || Disposing) return;
        refreshTimer.Enabled = IsHandleCreated && Visible;
        if (refreshTimer.Enabled) Synchronize();
    }

    internal void Synchronize()
    {
        if (synchronizing || IsDisposed || Disposing || !IsHandleCreated || !viewport.IsHandleCreated || !Visible) return;
        synchronizing = true;
        try
        {
            if (PageContentLayout.TryGet(viewport, out var layout)) layout.Clamp();
            SyncRail(vertical, false);
            SyncRail(horizontal, true);
            // The intersection is not an interactive part of either rail.
            var bounds = new Rectangle(vertical.Left, horizontal.Top, vertical.Width, horizontal.Height);
            bool showCorner = vertical.Visible && horizontal.Visible && bounds.Width > 0 && bounds.Height > 0 &&
                vertical.Bottom <= horizontal.Top + 1 && horizontal.Right <= vertical.Left + 1 &&
                ClientRectangle.Contains(bounds);
            if (showCorner)
            {
                corner.Bounds = bounds;
                corner.BackColor = TrackBackground;
                if (!corner.Visible) { corner.Visible = true; corner.BringToFront(); }
            }
            else if (corner.Visible) corner.Visible = false;
        }
        finally { synchronizing = false; }
    }

    private void SyncRail(Rail rail, bool across)
    {
        if (TryRead(across, out var bounds, out int page, out int maximum, out int position, out int step))
        {
            if (rail.Bounds != bounds) rail.Bounds = bounds;
            rail.UpdateMetrics(page, maximum, position, step);
            if (!rail.Visible) { rail.Visible = true; rail.BringToFront(); }
        }
        else if (rail.Visible) rail.Visible = false;
    }

    private bool TryRead(bool across, out Rectangle bounds, out int page, out int maximum, out int position, out int step)
    {
        bounds = Rectangle.Empty; page = 1; maximum = position = 0; step = 1;
        // DataGridView uses child ScrollBar controls rather than non-client bars.
        if (viewport is DataGridView grid)
        {
            var bar = grid.Controls.OfType<ScrollBar>().FirstOrDefault(s => across ? s is HScrollBar : s is VScrollBar);
            if (bar == null || !bar.Visible || bar.Width <= 0 || bar.Height <= 0) return false;
            bounds = RectangleToClient(bar.RectangleToScreen(bar.ClientRectangle));
            page = Math.Max(1, bar.LargeChange);
            maximum = Math.Max(0, bar.Maximum - page + 1 - bar.Minimum);
            position = Math.Max(0, Math.Min(maximum, bar.Value - bar.Minimum));
            step = Math.Max(1, bar.SmallChange);
            return true;
        }
        // ScrollableControl, EDIT, RichEdit and LISTBOX use native window bars.
        var info = new Native.ScrollBarInfo { Size = (uint)Marshal.SizeOf(typeof(Native.ScrollBarInfo)), State = new uint[6] };
        if (!Native.GetScrollBarInfo(viewport.Handle, across ? -6 : -5, ref info) ||
            (info.State[0] & (0x8000u | 0x10000u)) != 0) return false;
        bounds = RectangleToClient(Rectangle.FromLTRB(info.Rect.Left, info.Rect.Top, info.Rect.Right, info.Rect.Bottom));
        if (bounds.Width <= 0 || bounds.Height <= 0 || !ClientRectangle.Contains(bounds)) return false;
        var values = new Native.ScrollInfo { Size = (uint)Marshal.SizeOf(typeof(Native.ScrollInfo)), Mask = 7 };
        if (!Native.GetScrollInfo(viewport.Handle, across ? 0 : 1, ref values)) return true; // cover a visible but disabled native bar
        page = (int)Math.Max(1L, Math.Min(int.MaxValue, (long)values.Page));
        maximum = (int)Math.Max(0L, Math.Min(int.MaxValue, (long)values.Max - values.Min - page + 1));
        position = (int)Math.Max(0L, Math.Min(maximum, (long)values.Position - values.Min));
        if (PageContentLayout.TryGet(viewport, out var contentLayout) && viewport is Panel contentPage)
        {
            // The page range is derived from the measured last control, not a
            // potentially stale native virtual rectangle. Inner editors/grids
            // continue to use their own native ranges.
            page = Math.Max(1, across ? contentPage.ClientSize.Width : contentPage.ClientSize.Height);
            maximum = across ? 0 : contentLayout.Maximum;
            position = across ? 0 : Math.Max(0, Math.Min(maximum, -contentPage.AutoScrollPosition.Y));
        }
        if (viewport is TextBox edit && edit.Multiline && !across)
        {
            // Standard EDIT controls can expose a percentage-based native
            // scrollbar. EM_LINESCROLL, however, accepts actual line counts.
            int count = (int)Native.SendMessage(edit.Handle, 0x00BA, IntPtr.Zero, IntPtr.Zero); // EM_GETLINECOUNT
            int first = (int)Native.SendMessage(edit.Handle, 0x00CE, IntPtr.Zero, IntPtr.Zero); // EM_GETFIRSTVISIBLELINE
            var formatting = new Native.Rect();
            Native.SendRect(edit.Handle, 0x00B2, IntPtr.Zero, ref formatting); // EM_GETRECT
            int pixels = formatting.Bottom - formatting.Top;
            page = Math.Max(1, (pixels > 0 ? pixels : edit.ClientSize.Height) / Math.Max(1, edit.Font.Height));
            maximum = Math.Max(first, Math.Max(0, count - page));
            position = Math.Max(0, first);
        }
        // Panels, RichEdit and horizontal list bars are pixel-based. Native
        // multiline EDIT and vertical LISTBOX scroll in text lines/items.
        step = viewport is ScrollableControl || viewport is RichTextBox || across && viewport is ListBox
            ? Math.Max(1, (int)Math.Round(16 * DeviceDpi / 96.0)) : 1;
        return true;
    }

    internal void ScrollTo(bool across, int target)
    {
        if (viewport.IsDisposed || !viewport.Enabled || !viewport.IsHandleCreated) return;
        if (!TryRead(across, out _, out _, out int max, out int old, out _)) return;
        target = Math.Max(0, Math.Min(max, target));
        if (target == old) return;
        try
        {
            if (viewport is DataGridView grid)
            {
                if (across) grid.HorizontalScrollingOffset = target;
                else if (grid.Rows.Count > 0)
                {
                    // All application grids use ordinary unfrozen rows; filtered
                    // CSV rows are removed by its DataView rather than hidden.
                    int row = Math.Min(target, grid.Rows.Count - 1);
                    while (row < grid.Rows.Count && (!grid.Rows[row].Visible || grid.Rows[row].Frozen)) row++;
                    if (row < grid.Rows.Count) grid.FirstDisplayedScrollingRowIndex = row;
                }
            }
            else if (viewport is ScrollableControl sc)
            {
                var current = sc.AutoScrollPosition;
                sc.AutoScrollPosition = new Point(across ? target : -current.X, across ? -current.Y : target);
            }
            else if (viewport is ListBox && !across)
                Native.SendMessage(viewport.Handle, 0x0197, (IntPtr)target, IntPtr.Zero); // LB_SETTOPINDEX: full 32-bit index
            else if (viewport is RichTextBox)
            {
                // RichEdit exposes 32-bit pixel scroll coordinates; preserve
                // the other axis and use EM_SETSCROLLPOS instead of a 16-bit thumb.
                var p = new Native.Point();
                Native.SendPoint(viewport.Handle, 0x04DD, IntPtr.Zero, ref p); // EM_GETSCROLLPOS
                // Read the current rail positions too, because some RichEdit
                // versions truncate the coordinates returned by GETSCROLLPOS.
                if (TryRead(!across, out _, out _, out _, out int other, out _))
                { if (across) p.Y = other; else p.X = other; }
                if (across) p.X = target; else p.Y = target;
                Native.SendPoint(viewport.Handle, 0x04DE, IntPtr.Zero, ref p); // EM_SETSCROLLPOS
            }
            else if (viewport is TextBox && !across)
                Native.SendMessage(viewport.Handle, 0x00B6, IntPtr.Zero, (IntPtr)(target - old)); // EM_LINESCROLL
            else
            {
                // Win32 scroll messages carry a 16-bit thumb position. The
                // remaining horizontal cases are native file-list/edit bars;
                // use page/line messages beyond that range, without wrapping.
                int msg = across ? 0x0114 : 0x0115;
                if (target <= ushort.MaxValue)
                    Native.SendMessage(viewport.Handle, msg, (IntPtr)(5 | (target << 16)), IntPtr.Zero); // SB_THUMBTRACK
                else if (target == max) Native.SendMessage(viewport.Handle, msg, (IntPtr)7, IntPtr.Zero); // SB_BOTTOM/RIGHT
                else
                {
                    int delta = target - old;
                    int command = delta > 0 ? 3 : 2;
                    for (int i = 0; i < 16 && Math.Abs((long)delta) > 0; i++)
                    {
                        Native.SendMessage(viewport.Handle, msg, (IntPtr)command, IntPtr.Zero);
                        if (!TryRead(across, out _, out _, out _, out int now, out _) || now == old ||
                            (delta > 0 ? now >= target : now <= target)) break;
                        old = now; delta = target - old;
                    }
                }
                Native.SendMessage(viewport.Handle, msg, (IntPtr)8, IntPtr.Zero); // SB_ENDSCROLL
            }
        }
        // A data binding/reset can invalidate a row during a scroll event.
        catch (ArgumentOutOfRangeException) { }
        catch (InvalidOperationException) { }
        Synchronize();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { refreshTimer.Stop(); refreshTimer.Dispose(); }
        base.Dispose(disposing);
    }

    // Shared geometry; used in both orientations and by regression tests.
    internal static Rectangle ThumbBounds(int height, int width, int minimumThumb, int viewportHeight,
        int maximum, int position, int inset)
    {
        int travelArea = Math.Max(0, height - inset * 2);
        if (travelArea == 0 || width <= 0) return Rectangle.Empty;
        int thumb = maximum <= 0 ? travelArea : Math.Min(travelArea,
            Math.Max(minimumThumb, (int)Math.Round(travelArea * (double)viewportHeight / (viewportHeight + (double)maximum))));
        int travel = travelArea - thumb;
        int y = inset + (maximum <= 0 ? 0 : (int)Math.Round(travel * (double)Math.Max(0, Math.Min(maximum, position)) / maximum));
        return new Rectangle(0, y, width, thumb);
    }

    private sealed class Rail : Control
    {
        private readonly ModernScrollHost host;
        private readonly bool across;
        private bool hot, dragging;
        private int dragOffset, wheelRemainder;
        private int maximum, position, pageSize = 1, smallChange = 1;
        internal Rail(ModernScrollHost host, bool across)
        {
            this.host = host; this.across = across;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            AccessibleName = across ? "Horizontal scrollbar" : "Vertical scrollbar";
            AccessibleRole = AccessibleRole.ScrollBar;
            Tag = "inherit"; TabStop = false;
        }
        private int ScalePixel(int n) => Math.Max(1, (int)Math.Round(n * DeviceDpi / 96.0));
        private int Inset => ScalePixel(3);
        private int Length => across ? Width : Height;
        private int Thickness => across ? Height : Width;
        private Rectangle Thumb => ThumbBounds(Length, Thickness, ScalePixel(24), pageSize, maximum, position, Inset);
        private int Axis(Point p) => across ? p.X : p.Y;
        private Rectangle Orient(Rectangle r) => across ? new Rectangle(r.Y, r.X, r.Height, r.Width) : r;
        internal void UpdateMetrics(int page, int max, int pos, int step)
        {
            if (page != pageSize || max != maximum || pos != position || step != smallChange)
            { pageSize = page; maximum = max; position = pos; smallChange = step; TabStop = max > 0; Invalidate(); }
        }
        private void MoveTo(long value) => host.ScrollTo(across, (int)Math.Max(0, Math.Min((long)maximum, value)));
        private void Step(long amount) => MoveTo(position + amount);
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(host.TrackBackground);
            if (maximum <= 0) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            bool expanded = hot || dragging || Focused;
            if (expanded && Length > 4 && Thickness > 4)
            {
                var track = Orient(new Rectangle(ScalePixel(2), 1, Math.Max(1, Thickness - ScalePixel(4)), Length - 2));
                using var trackPath = Ui.Round(track, ScalePixel(5));
                using var trackBrush = new SolidBrush(Theme.Dark ? Color.FromArgb(45,45,45) : Color.FromArgb(230,230,230));
                e.Graphics.FillPath(trackBrush, trackPath);
            }
            var thumb = Thumb;
            if (thumb.Height < 2) return;
            int width = Math.Min(Math.Max(1, Thickness - 2), ScalePixel(expanded ? 8 : 4));
            thumb.X = (Thickness - width) / 2; thumb.Width = width;
            Color color = Theme.Dark ? (expanded ? Color.FromArgb(180,180,180) : Color.FromArgb(119,119,119))
                                     : (expanded ? Color.FromArgb(92,92,92) : Color.FromArgb(145,145,145));
            if (!Enabled) color = Theme.Border;
            using var path = Ui.Round(Orient(thumb), Math.Max(1,width/2));
            using var brush = new SolidBrush(color);
            e.Graphics.FillPath(brush,path);
        }
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hot = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hot = false; Invalidate(); }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || maximum <= 0) return;
            Focus(); host.Synchronize();
            var thumb = Thumb; int coordinate = Axis(e.Location);
            if (coordinate >= thumb.Top && coordinate < thumb.Bottom)
            { dragging = true; dragOffset = coordinate - thumb.Top; Capture = true; Invalidate(); }
            else Step(coordinate < thumb.Top ? -pageSize : pageSize);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e); if (!dragging) return;
            int travel = Length - 2 * Inset - Thumb.Height;
            if (travel > 0) MoveTo((long)Math.Round((Axis(e.Location) - dragOffset - Inset) * (double)maximum / travel));
        }
        protected override void OnMouseUp(MouseEventArgs e)
        { base.OnMouseUp(e); if (e.Button == MouseButtons.Left) { dragging = false; Capture = false; Invalidate(); } }
        protected override void OnMouseCaptureChanged(EventArgs e)
        { base.OnMouseCaptureChanged(e); if (!Capture) { dragging = false; Invalidate(); } }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e is HandledMouseEventArgs handled) handled.Handled = true;
            wheelRemainder += e.Delta;
            int notches = wheelRemainder / 120; wheelRemainder %= 120;
            int lines = SystemInformation.MouseWheelScrollLines;
            if (notches != 0 && lines != 0) Step(-(long)notches * (lines < 0 ? pageSize : (long)lines * smallChange));
        }
        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            { case Keys.Up: case Keys.Down: case Keys.Left: case Keys.Right: case Keys.PageUp: case Keys.PageDown: case Keys.Home: case Keys.End: return true; }
            return base.IsInputKey(keyData);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up: case Keys.Left: Step(-smallChange); break;
                case Keys.Down: case Keys.Right: Step(smallChange); break;
                case Keys.PageUp: Step(-pageSize); break;
                case Keys.PageDown: Step(pageSize); break;
                case Keys.Home: MoveTo(0); break;
                case Keys.End: MoveTo(maximum); break;
                default: base.OnKeyDown(e); return;
            }
            e.Handled = true; e.SuppressKeyPress = true;
        }
    }

    private static class Native
    {
        [StructLayout(LayoutKind.Sequential)] internal struct Rect { internal int Left, Top, Right, Bottom; }
        [StructLayout(LayoutKind.Sequential)] internal struct Point { internal int X, Y; }
        [StructLayout(LayoutKind.Sequential)] internal struct ScrollBarInfo
        {
            internal uint Size; internal Rect Rect; internal int LineButton, ThumbTop, ThumbBottom, Reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] internal uint[] State;
        }
        [StructLayout(LayoutKind.Sequential)] internal struct ScrollInfo
        { internal uint Size, Mask; internal int Min, Max; internal uint Page; internal int Position, TrackPosition; }
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetScrollBarInfo(IntPtr hwnd, int objectId, ref ScrollBarInfo info);
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetScrollInfo(IntPtr hwnd, int bar, ref ScrollInfo info);
        [DllImport("user32.dll", EntryPoint = "SendMessageW")] internal static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", EntryPoint = "SendMessageW")] internal static extern IntPtr SendRect(IntPtr hwnd, int msg, IntPtr wParam, ref Rect rect);
        [DllImport("user32.dll", EntryPoint = "SendMessageW")] internal static extern IntPtr SendPoint(IntPtr hwnd, int msg, IntPtr wParam, ref Point point);
    }
}
