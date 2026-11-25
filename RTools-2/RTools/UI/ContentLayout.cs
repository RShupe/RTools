namespace RTools.UI;

// Runtime-only layout. Measure every group at its real available width; never
// let a previously scrolled display rectangle or a fill-docked editor choose
// the height of an auto-sizing ancestor.
internal enum ContentDirection { Stack, Row, Columns }

internal sealed class ContentGroup : Panel
{
    private readonly ContentDirection direction;
    private readonly int autoColumn;
    private readonly bool designerSizing;
    internal (SizeType Type, float Width)[]? DesignerColumns { get; set; }
    private readonly Dictionary<Control, Size> originalSizes = new();
    private bool arranging;
    internal event EventHandler? ContentChanged;

    internal ContentGroup(ContentDirection direction, int autoColumn = -1, bool designerSizing = false)
    {
        this.direction = direction;
        this.autoColumn = autoColumn;
        this.designerSizing = designerSizing;
        AutoSize = false;
        Margin = Padding.Empty;
        TabStop = false;
        Tag = "inherit";
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
    }

    internal void AddItem(Control child)
    {
        originalSizes.Add(child, child.Size);
        child.Dock = DockStyle.None;
        child.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        // The measuring pass controls these bounds, including wrapped labels.
        if (child is Label || child is ButtonBase) child.AutoSize = false;
        if (child is Label) child.MaximumSize = Size.Empty;
        child.VisibleChanged += Changed;
        child.FontChanged += Changed;
        child.MarginChanged += Changed;
        if (child is Label || child is ButtonBase) child.TextChanged += Changed;
        if (child is PictureBox) child.SizeChanged += Changed;
        if (child is ContentGroup group) group.ContentChanged += Changed;
        Controls.Add(child);
        Changed(this, EventArgs.Empty);
    }

    private void Changed(object? sender, EventArgs e)
    {
        if (!arranging) ContentChanged?.Invoke(this, EventArgs.Empty);
    }

    private int NaturalWidth(Control child)
    {
        if (child is ContentGroup group) return group.NaturalWidth();
        if (child is PictureBox picture) return Math.Max(originalSizes[child].Width, picture.Width);
        if (child is Label label)
            return Math.Max(child.MinimumSize.Width, TextRenderer.MeasureText(label.Text, label.Font).Width + label.Padding.Horizontal);
        if (child is ButtonBase)
            return Math.Max(Math.Max(child.MinimumSize.Width, child.GetPreferredSize(Size.Empty).Width), designerSizing ? originalSizes[child].Width : 0);
        return Math.Max(child.MinimumSize.Width, originalSizes[child].Width);
    }

    private int NaturalWidth()
    {
        var widths = Controls.Cast<Control>().Where(c => c.Visible).Select(c => NaturalWidth(c) + c.Margin.Horizontal).ToArray();
        return Padding.Horizontal + (widths.Length == 0 ? 0 : direction == ContentDirection.Stack ? widths.Max() : widths.Sum());
    }

    private int HeightFor(Control child, int width, bool arrange)
    {
        if (child is ContentGroup group) return group.Fit(width, arrange);
        if (child is Label label)
        {
            var proposed = new Size(Math.Max(1, width - label.Padding.Horizontal), int.MaxValue);
            var size = TextRenderer.MeasureText(label.Text, label.Font, proposed,
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPrefix);
            return Math.Max(label.Font.Height, size.Height) + label.Padding.Vertical;
        }
        if (child is ButtonBase)
            return Math.Max(Math.Max(child.MinimumSize.Height, child.GetPreferredSize(new Size(Math.Max(1, width), 0)).Height), designerSizing ? originalSizes[child].Height : 0);
        // Fixed-height lists, editors, grids and swatches must not inherit an
        // inflated row height from a previous layout pass.
        if (child is PictureBox picture) return Math.Max(originalSizes[child].Height, picture.Height);
        return Math.Max(child.MinimumSize.Height, originalSizes[child].Height);
    }

    private int Place(Control child, int x, int y, int width, bool arrange)
    {
        width = Math.Max(1, width);
        int height = HeightFor(child, width, arrange);
        if (arrange)
        {
            var bounds = new Rectangle(x, y, width, height);
            if (child.Bounds != bounds) child.Bounds = bounds;
        }
        return height;
    }

    // Measure and optionally arrange using identical calculations. Return the
    // bottom of the actual content, not the viewport height or scroll offset.
    internal int Fit(int width, bool arrange)
    {
        int available = Math.Max(1, width - Padding.Horizontal);
        var items = Controls.Cast<Control>().Where(c => c.Visible).ToArray();
        var designerColumns = DesignerColumns;
        int y = Padding.Top;
        if (arrange) { arranging = true; SuspendLayout(); }
        try
        {
            if (direction == ContentDirection.Stack)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    var c = items[i]; var m = c.Margin;
                    y += m.Top;
                    y += Place(c, Padding.Left + m.Left, y, available - m.Horizontal, arrange);
                    // Keep inter-control spacing, but never reserve a blank row
                    // or an extra card margin after the final visible control.
                    if (i + 1 < items.Length) y += m.Bottom;
                }
            }
            else if (direction == ContentDirection.Row)
            {
                int x = 0, rowHeight = 0, lastBottom = 0;
                foreach (var c in items)
                {
                    var m = c.Margin;
                    int w = Math.Min(Math.Max(1, available - m.Horizontal), Math.Max(1, NaturalWidth(c)));
                    if (x > 0 && x + w + m.Horizontal > available)
                    { y += rowHeight; x = 0; rowHeight = 0; lastBottom = 0; }
                    int h = Place(c, Padding.Left + x + m.Left, y + m.Top, w, arrange);
                    x += w + m.Horizontal;
                    rowHeight = Math.Max(rowHeight, m.Top + h + m.Bottom);
                    lastBottom = Math.Max(lastBottom, m.Top + h);
                }
                y += lastBottom;
            }
            else if (items.Length > 0 && designerColumns != null)
            {
                // Read widths from the actual designer ColumnStyles. Absolute
                // and auto columns reserve their preferred share; percentage
                // columns divide the remaining width without scroll feedback.
                var specs = items.Select(c =>
                {
                    int i = Controls.GetChildIndex(c);
                    return i >= 0 && i < designerColumns.Length ? designerColumns[i] : (SizeType.Percent, 100f);
                }).ToArray();
                var desired = items.Select((c, i) => specs[i].Item1 == SizeType.Absolute
                    ? Math.Max(1, (int)Math.Round(specs[i].Item2))
                    : specs[i].Item1 == SizeType.AutoSize ? NaturalWidth(c) + c.Margin.Horizontal : 0).ToArray();
                int fixedTotal = desired.Sum();
                int flexible = specs.Count(x => x.Item1 == SizeType.Percent);
                int fixedBudget = flexible == 0 ? Math.Min(fixedTotal, available) : Math.Min(fixedTotal, Math.Max(0, available - Math.Min(available / 2, 80 * flexible)));
                double remainingWeight = specs.Where(x => x.Item1 == SizeType.Percent).Sum(x => (double)Math.Max(1, x.Item2));
                int remainingFixed = fixedBudget, remainingDesired = fixedTotal, remainingFlex = Math.Max(0, available - fixedBudget), xPos = Padding.Left, rowBottom = 0;
                for (int i = 0; i < items.Length; i++)
                {
                    var c = items[i]; var m = c.Margin; int cell;
                    if (specs[i].Item1 == SizeType.Percent)
                    {
                        double weight = Math.Max(1, specs[i].Item2);
                        cell = remainingWeight <= weight ? remainingFlex : (int)(remainingFlex * weight / remainingWeight);
                        remainingFlex -= cell; remainingWeight -= weight;
                    }
                    else
                    {
                        cell = remainingDesired <= desired[i] ? remainingFixed : (int)((double)remainingFixed * desired[i] / remainingDesired);
                        remainingFixed -= cell; remainingDesired -= desired[i];
                    }
                    int h = Place(c, xPos + m.Left, y + m.Top, cell - m.Horizontal, arrange);
                    rowBottom = Math.Max(rowBottom, m.Top + h);
                    xPos += cell;
                }
                y += rowBottom;
            }
            else if (items.Length > 0)
            {
                var fixedChild = autoColumn >= 0 && autoColumn < Controls.Count ? Controls[autoColumn] : null;
                if (fixedChild != null && !fixedChild.Visible) fixedChild = null;
                int fixedWidth = fixedChild == null ? 0 : Math.Min(available, NaturalWidth(fixedChild) + fixedChild.Margin.Horizontal);
                // Preserve at least a usable share for the flexible column.
                if (items.Length > 1) fixedWidth = Math.Min(fixedWidth, available / 2);
                int flexibleCount = items.Length - (fixedChild == null ? 0 : 1);
                int remaining = available - fixedWidth, x = Padding.Left, rowBottom = 0;
                foreach (var c in items)
                {
                    var m = c.Margin;
                    int cell;
                    if (c == fixedChild) cell = fixedWidth;
                    else { cell = remaining / Math.Max(1, flexibleCount); remaining -= cell; flexibleCount--; }
                    int h = Place(c, x + m.Left, y + m.Top, cell - m.Horizontal, arrange);
                    rowBottom = Math.Max(rowBottom, m.Top + h);
                    x += cell;
                }
                y += rowBottom;
            }
            return y + Padding.Bottom;
        }
        finally
        {
            if (arrange) { ResumeLayout(false); arranging = false; }
        }
    }
}

// Keeps the native WinForms page scroll range tied to the one real content
// child. The modern overlay can then use a bounded, pixel-based scroll range.
internal sealed class PageContentLayout
{
    internal const int BottomPadding = 12;
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control, PageContentLayout> layouts = new();
    private readonly Panel page;
    private readonly Control body;
    private readonly ContentGroup? group;
    private readonly Padding inset;
    private readonly int dashboardHeight;
    private bool pending, dirty = true, updating;
    private Size measuredSize = Size.Empty, measuredClientSize = Size.Empty;
    private int contentBottom;

    private PageContentLayout(Panel page, Control body, Padding inset)
    {
        this.page = page; this.body = body; this.inset = inset;
        group = body as ContentGroup ?? (body is UserControl ? body.Controls.OfType<ContentGroup>().SingleOrDefault() : null);
        dashboardHeight = body.Height;
        page.Padding = Padding.Empty;
        page.AutoScrollMargin = Size.Empty;
        page.AutoScrollMinSize = Size.Empty;
        page.AutoScroll = true;
        body.AutoSize = false;
        body.Dock = DockStyle.None;
        body.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        page.Layout += (_, _) => Request();
        page.SizeChanged += (_, _) => Request();
        page.ClientSizeChanged += (_, _) => Request();
        page.HandleCreated += (_, _) => Invalidate();
        page.VisibleChanged += (_, _) => { if (page.Visible) Invalidate(); };
        page.Scroll += (_, _) => Clamp();
        page.MouseWheel += (_, _) => Clamp();
        if (group != null) group.ContentChanged += (_, _) => Invalidate();
    }

    internal static void Attach(Panel page, Control body, Padding inset)
    {
        var layout = new PageContentLayout(page, body, inset);
        layouts.Add(page, layout);
        layout.Request();
    }

    internal static bool TryGet(Control page, out PageContentLayout layout) => layouts.TryGetValue(page, out layout);
    internal int Maximum => Math.Max(0, contentBottom - page.ClientSize.Height);
    internal int ContentBottom => contentBottom;
    private void Invalidate() { dirty = true; Request(); }

    private void Request()
    {
        if (updating || pending || page.IsDisposed || !page.IsHandleCreated || !page.Visible) return;
        if (!dirty && page.Size == measuredSize && page.ClientSize == measuredClientSize) return;
        pending = true;
        page.BeginInvoke((Action)(() => { pending = false; Refresh(); }));
    }

    internal void Refresh()
    {
        if (updating || page.IsDisposed || !page.IsHandleCreated || !page.Visible || page.Width <= inset.Horizontal || page.Height <= 0) return;
        if (!dirty && page.Size == measuredSize && page.ClientSize == measuredClientSize) { Clamp(); return; }
        updating = true;
        try
        {
            int oldY = Math.Max(0, -page.AutoScrollPosition.Y);
            int width = Math.Max(1, page.Width - inset.Horizontal);
            int height = group?.Fit(width, false) ?? dashboardHeight;
            bool needsScroll = height + inset.Vertical > page.Height;
            if (needsScroll)
            {
                int nativeWidth = page.VerticalScroll.Visible ? page.Width - page.ClientSize.Width : 0;
                int bar = nativeWidth > 0 ? nativeWidth : SystemInformation.VerticalScrollBarWidth;
                width = Math.Max(1, width - bar);
            }
            height = group?.Fit(width, true) ?? dashboardHeight;
            contentBottom = inset.Top + height + inset.Bottom;
            page.SuspendLayout();
            try
            {
                // Bounds are in the currently scrolled coordinate system. Do
                // not add the current scroll offset to the logical height.
                var offset = page.AutoScrollPosition;
                body.Bounds = new Rectangle(inset.Left + offset.X, inset.Top + offset.Y, width, height);
                if (group == null) body.PerformLayout();
                else if (!ReferenceEquals(body, group)) group.Bounds = new Rectangle(0, 0, width, height);
                page.AutoScrollMinSize = new Size(0, contentBottom);
            }
            finally { page.ResumeLayout(true); }
            // The actual native scrollbar width can differ at high DPI. Use
            // the real client area on the second pass, never compound a reserve.
            int actualWidth = Math.Max(1, page.ClientSize.Width - inset.Horizontal);
            if (actualWidth != width)
            {
                height = group?.Fit(actualWidth, true) ?? dashboardHeight;
                contentBottom = inset.Top + height + inset.Bottom;
                var offset = page.AutoScrollPosition;
                body.Bounds = new Rectangle(inset.Left + offset.X, inset.Top + offset.Y, actualWidth, height);
                if (group != null && !ReferenceEquals(body, group)) group.Bounds = new Rectangle(0, 0, actualWidth, height);
                page.AutoScrollMinSize = new Size(0, contentBottom);
                page.PerformLayout();
            }
            page.AutoScrollPosition = new Point(0, Math.Min(oldY, Maximum));
            measuredSize = page.Size;
            measuredClientSize = page.ClientSize;
            dirty = false;
        }
        finally { updating = false; }
        if (page.Parent is ModernScrollHost host) host.Synchronize();
    }

    internal void Clamp()
    {
        if (updating || page.IsDisposed || !page.IsHandleCreated || !page.Visible) return;
        int y = Math.Max(0, -page.AutoScrollPosition.Y);
        if (y > Maximum || page.AutoScrollPosition.X != 0)
            page.AutoScrollPosition = new Point(0, Math.Min(y, Maximum));
    }
}
