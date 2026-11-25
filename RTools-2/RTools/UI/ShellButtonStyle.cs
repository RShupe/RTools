using System.Drawing.Drawing2D;

namespace RTools.UI
{
    internal enum ShellButtonKind { Navigation, Card, Action, Primary }

    // Runtime-only decoration of a stock WinForms Button. No custom controls,
    // constructors, or paint handlers are serialized in MainForm.Designer.cs.
    internal sealed class ShellButtonStyle
    {
        private readonly Button button;
        private readonly ShellButtonKind kind;
        private readonly string glyph;
        private bool hover, pressed, selected;

        internal bool Selected
        {
            get { return selected; }
            set { if (selected != value) { selected = value; button.Invalidate(); } }
        }

        internal ShellButtonStyle(Button button, ShellButtonKind kind, string glyph)
        {
            this.button = button;
            this.kind = kind;
            this.glyph = glyph;
            button.FlatStyle = FlatStyle.Flat;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;
            button.Paint += Paint;
            button.MouseEnter += (_, _) => { hover = true; button.Invalidate(); };
            button.MouseLeave += (_, _) => { hover = pressed = false; button.Invalidate(); };
            button.MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) { pressed = true; button.Invalidate(); } };
            button.MouseUp += (_, _) => { pressed = false; button.Invalidate(); };
            button.MouseCaptureChanged += (_, _) => { if (!button.Capture) { pressed = false; button.Invalidate(); } };
            button.GotFocus += (_, _) => button.Invalidate();
            button.LostFocus += (_, _) => { pressed = false; button.Invalidate(); };
            button.EnabledChanged += (_, _) => { hover = pressed = false; button.Invalidate(); };
            button.KeyDown += (_, e) => { if (e.KeyCode == Keys.Space) { pressed = true; button.Invalidate(); } };
            button.KeyUp += (_, e) => { if (e.KeyCode == Keys.Space) { pressed = false; button.Invalidate(); } };
        }

        private int D(int value) => Math.Max(1, (int)Math.Round(value * button.DeviceDpi / 96.0));

        private void Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var w = button.ClientSize.Width;
            var h = button.ClientSize.Height;
            if (w < 4 || h < 4) return;
            var parent = button.Parent?.BackColor ?? Theme.Bg;
            bool card = kind == ShellButtonKind.Card;
            bool nav = kind == ShellButtonKind.Navigation;
            bool primary = kind == ShellButtonKind.Primary;
            bool enabled = button.Enabled;
            bool hot = enabled && (hover || pressed);
            g.Clear(parent);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = new Rectangle(1, 1, w - 3, h - 3);
            Color fill = !enabled ? Theme.Input : primary ? (pressed ? Theme.Primary : hover ? Color.FromArgb(166, 34, 57) : Theme.Primary) : pressed ? Theme.Highlight : card ? (hover ? Theme.Highlight : Theme.Surface) : nav ? (selected ? Theme.Highlight : hover ? Theme.Input : parent) : hover ? Theme.Highlight : Theme.Surface;
            Color fg = !enabled ? Theme.Muted : primary ? Color.White : selected || (card && hot) ? Theme.SelectedText : Theme.Ink;
            Color stroke = enabled && (hot || button.Focused) ? Theme.Accent : Theme.Border;
            using (var shape = Ui.Round(bounds, D(8)))
            using (var brush = new SolidBrush(fill))
            using (var pen = new Pen(stroke, button.Focused ? D(2) : 1))
            {
                g.FillPath(brush, shape);
                if ((!nav && !primary) || button.Focused) g.DrawPath(pen, shape);
            }
            if (nav && selected)
            {
                using var pen = new Pen(Theme.Accent, D(3)) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                g.DrawLine(pen, D(5), h / 3, D(5), h * 2 / 3);
            }
            var leftFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;
            var centerFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;
            if (card)
            {
                // Text remains editable in the designer: line 1 is the heading,
                // the remaining lines form the description. The entire tile is clickable.
                var text = button.Text.Replace("\r", "").Split(new[] { '\n' }, 2);
                int icon = Math.Min(D(36), Math.Max(D(20), h - D(16)));
                int left = D(13), textX = left + icon + D(12), end = D(31);
                var iconRect = new Rectangle(left, (h - icon) / 2, icon, icon);
                using (var iconPath = Ui.Round(iconRect, D(8)))
                using (var iconBrush = new SolidBrush(Theme.Highlight)) g.FillPath(iconBrush, iconPath);
                using (var iconFont = new Font("Segoe MDL2 Assets", 15f))
                    TextRenderer.DrawText(g, glyph, iconFont, iconRect, enabled ? Theme.SelectedText : Theme.Muted, centerFlags);
                var title = text[0];
                var description = text.Length > 1 ? text[1].Replace('\n', ' ') : "";
                int textWidth = Math.Max(1, w - textX - end);
                // Card text must never draw an ellipsis beside the arrow. Wrap the
                // description at narrow widths and keep it within the card's text area.
                var titleFlags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
                var descriptionFlags = TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak;
                using var titleFont = new Font(button.Font, FontStyle.Bold);
                using var descFont = new Font(button.Font.FontFamily, Math.Max(8f, button.Font.SizeInPoints - 1), FontStyle.Regular);
                int titleHeight = Math.Max(D(18), titleFont.Height + D(2));
                int descHeight = description.Length == 0 ? 0 : Math.Min(Math.Max(0, h - D(8) - titleHeight),
                    TextRenderer.MeasureText(g, description, descFont, new Size(textWidth, int.MaxValue), descriptionFlags).Height);
                int top = Math.Max(D(3), (h - titleHeight - descHeight) / 2);
                TextRenderer.DrawText(g, title, titleFont, new Rectangle(textX, top, textWidth, titleHeight), fg, titleFlags);
                if (descHeight > 0)
                    TextRenderer.DrawText(g, description, descFont, new Rectangle(textX, top + titleHeight, textWidth, descHeight), Theme.Muted, descriptionFlags);
                // Draw one simple chevron, independent of icon-font availability.
                int arrowX = w - D(16), arrowY = h / 2;
                using var arrowPen = new Pen(Theme.Muted, D(2)) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
                g.DrawLines(arrowPen, new[] { new Point(arrowX - D(3), arrowY - D(5)), new Point(arrowX + D(2), arrowY), new Point(arrowX - D(3), arrowY + D(5)) });
            }
            else if (nav)
            {
                int left = glyph.Length > 0 ? D(43) : D(14);
                if (glyph.Length != 0)
                {
                    using var glyphFont = new Font("Segoe MDL2 Assets", 13f);
                    TextRenderer.DrawText(g, glyph, glyphFont, new Rectangle(D(13), 0, D(27), h), fg, centerFlags);
                }
                TextRenderer.DrawText(g, button.Text, button.Font, new Rectangle(left, 0, Math.Max(1, w - left - D(8)), h), fg, leftFlags);
            }
            else
                TextRenderer.DrawText(g, button.Text, button.Font, new Rectangle(D(6), 0, Math.Max(1, w - D(12)), h), fg, centerFlags);
        }
    }
}
