using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using RTools.Core;

namespace RTools.UI.Pages
{
    internal static class ColorPickerPageRuntime
    {
        internal static Panel Build(ColorPickerPage view, MainForm window)
        {
        var ui = DesignerPageRuntime.Attach(view);
        var page = ui.Page;var hex = ui.Get<TextBox>("hex");var red = ui.Get<ModernNumber>("red");var green = ui.Get<ModernNumber>("green");var blue = ui.Get<ModernNumber>("blue");var h = ui.Get<TextBox>("h");var s = ui.Get<TextBox>("s");var l = ui.Get<TextBox>("l");var rgb = ui.Get<TextBox>("rgb");var hsl = ui.Get<TextBox>("hsl");var swatch = ui.Get<Panel>("swatch");var sliders = new[] { ui.Get<ModernSlider>("slider0"), ui.Get<ModernSlider>("slider1"), ui.Get<ModernSlider>("slider2") };bool updating=false;
        void Render(Rgb c){updating=true;try{hex.Text=c.Hex;red.Value=sliders[0].Value=c.R;green.Value=sliders[1].Value=c.G;blue.Value=sliders[2].Value=c.B;swatch.BackColor=Color.FromArgb(c.R,c.G,c.B);var (hh,ss,ll)=c.Hsl;h.Text=hh.ToString("0.0",CultureInfo.InvariantCulture);s.Text=ss.ToString("0.0",CultureInfo.InvariantCulture);l.Text=ll.ToString("0.0",CultureInfo.InvariantCulture);rgb.Text=c.Css;hsl.Text=c.HslCss;}finally{updating=false;}}
        foreach(var box in new[]{red,green,blue})box.ValueChanged+=(_,_)=>{if(!updating)Render(new Rgb((byte)red.Value,(byte)green.Value,(byte)blue.Value));};foreach(var slider in sliders)slider.ValueChanged+=(_,_)=>{if(!updating)Render(new Rgb((byte)sliders[0].Value,(byte)sliders[1].Value,(byte)sliders[2].Value));};
        double Parse(TextBox b)=>double.TryParse(b.Text,NumberStyles.Float,CultureInfo.InvariantCulture,out var n)?n:throw new ArgumentException("Enter a numeric HSL value using a decimal point.");
        ui.BindClick("applyHEXButton", ()=>Render(Rgb.Parse(hex.Text)));
        ui.BindClick("chooseColorButton", ()=>{using var d=new ColorDialog{Color=swatch.BackColor,FullOpen=true};if(d.ShowDialog(window)==DialogResult.OK)Render(new Rgb(d.Color.R,d.Color.G,d.Color.B));});
        ui.BindClick("copyHEXButton", ()=>Ui.Copy(hex.Text));
        ui.BindClick("applyHSLButton", ()=>Render(Rgb.FromHsl(Parse(h),Parse(s),Parse(l))));
        ui.BindClick("copyRGBButton", ()=>Ui.Copy(rgb.Text));
        ui.BindClick("copyHSLButton", ()=>Ui.Copy(hsl.Text));Render(new Rgb((byte)red.Value,(byte)green.Value,(byte)blue.Value));return page;
        }
    }
}
