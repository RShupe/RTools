using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RTools.UI
{
    [ToolboxItem(true)]
    public class ModernButton : Button
    {
        [DefaultValue(false)]
        public bool PrimaryAction { get; set; }
        [DefaultValue(false)]
        public bool Navigation { get; set; }
        [DefaultValue(false)]
        public bool Selected { get; set; }
        [DefaultValue("")]
        public string Glyph { get; set; } = "";
        private bool hover, pressed;
        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0; UseVisualStyleBackColor = false;
            Cursor = Cursors.Hand; AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MinimumSize = new Size(92,34); Padding = new Padding(13,5,13,5); Margin = new Padding(0,0,8,7);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }
        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; e.Graphics.Clear(Parent?.BackColor ?? Theme.Bg);
            var r = new Rectangle(1,1,Math.Max(1,Width-3),Math.Max(1,Height-3));
            using var shape = Ui.Round(r, 7);
            var bg = !Enabled ? Theme.Input : Navigation ? Selected ? Theme.Highlight : hover ? Theme.Input : Parent?.BackColor ?? Theme.Sidebar : PrimaryAction ? (pressed ? Theme.Primary : hover ? Color.FromArgb(166,34,57) : Theme.Primary) : hover ? Theme.Input : Theme.Surface;
            var fg = !Enabled ? Theme.Muted : PrimaryAction ? Color.White : Selected ? Theme.SelectedText : Theme.Ink;
            using (var brush = new SolidBrush(bg)) e.Graphics.FillPath(brush,shape);
            if (!Navigation && !PrimaryAction) using (var pen = new Pen(Theme.Border)) e.Graphics.DrawPath(pen,shape);
            if (Navigation && Selected) using (var pen = new Pen(Theme.Accent,3)) { pen.StartCap = pen.EndCap = LineCap.Round; e.Graphics.DrawLine(pen,4,Height/3,4,Height*2/3); }
            var textRect = new Rectangle(Navigation ? (Glyph.Length == 0 ? 14 : 43) : 9,0,Math.Max(1,Width-(Navigation ? 50 : 18)),Height);
            if (Navigation && Glyph.Length > 0) using (var font = new Font("Segoe MDL2 Assets",14f)) TextRenderer.DrawText(e.Graphics,Glyph,font,new Rectangle(13,0,27,Height),fg,TextFormatFlags.VerticalCenter|TextFormatFlags.HorizontalCenter|TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(e.Graphics,Text,Font,textRect,fg,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix|(Navigation ? TextFormatFlags.Left : TextFormatFlags.HorizontalCenter));
            if (Focused && ShowFocusCues) using (var pen = new Pen(Theme.Accent,2)) e.Graphics.DrawPath(pen,shape);
        }
    }
}
