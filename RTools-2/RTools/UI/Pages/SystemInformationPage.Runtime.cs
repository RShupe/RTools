using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using RTools.Core;

namespace RTools.UI.Pages
{
    internal static class SystemInformationPageRuntime
    {
        internal static Panel Build(SystemInformationPage view, MainForm window)
        {
        var ui = DesignerPageRuntime.Attach(view);
        var page = ui.Page;var result = ui.Get<RichTextBox>("result");
        ui.BindAsync("refreshbutton", async()=>await window.Run((t,p)=>WindowsSystemInfo.Collect(t,p),s=>result.Text=s));
        ui.BindClick("copyAllButton", ()=>Ui.Copy(result.Text));
        ui.BindClick("saveReportButton", ()=>{using var d=new SaveFileDialog{Filter="Text files (*.txt)|*.txt",DefaultExt="txt"};if(d.ShowDialog(window)==DialogResult.OK)File.WriteAllText(d.FileName,result.Text); });return page;
        }
    }
}
