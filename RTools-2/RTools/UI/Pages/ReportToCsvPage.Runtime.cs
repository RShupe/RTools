using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using RTools.Core;

namespace RTools.UI.Pages
{
    internal static class ReportToCsvPageRuntime
    {
        internal static Panel Build(ReportToCsvPage view, MainForm window)
        {
        bool Confirm(string s) => Ui.Confirm(window, s);
        bool Overwrite(string path) => Ui.Overwrite(window, path);
        var ui = DesignerPageRuntime.Attach(view);
        var page = ui.Page;var input = ui.Get<TextBox>("input");var output = ui.Get<TextBox>("output");var header = ui.Get<ModernNumber>("header");var debit = ui.Get<TextBox>("debit");var other = ui.Get<TextBox>("other");var mask = ui.Get<ModernCheckBox>("mask");var summary = ui.Get<Label>("summary");
        ReportOptions Options()=>new((int)header.Value,debit.Text.Trim(),other.Text.Trim(),mask.Checked);
        void Result(ReportResult r)=>summary.Text=$"{r.Exportable:N0} exportable · {r.Ignored:N0} ignored · {r.OtherCards:N0} other cards · {r.Malformed:N0} malformed";
        ui.BindPath("input", "inputbrowsebutton", "Transaction report", false, Ui.All, false);
        ui.BindPath("output", "outputbrowsebutton", "Output CSV", true, Ui.Csv, false);
        ui.BindAsync("previewbutton", async()=>{var src=input.Text;var o=Options();await window.Run((t,p)=>ReportExtractor.Preview(src,o,t,p),Result);});
        ui.BindAsync("exportCSVButton", async()=>{var src=input.Text;var dst=FileTools.Output(output.Text);var o=Options();if(!o.MaskIdentifiers&&!Confirm("Export unmasked card/account identifiers? Only proceed if authorized to store this data."))return;if(!Overwrite(dst))return;await window.Run((t,p)=>ReportExtractor.Export(src,dst,o,t,p),r=>{Result(r);summary.Text+="\nExported to "+dst;});});return page;
        }
    }
}
