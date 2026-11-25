using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using RTools.Core;

namespace RTools.UI.Pages
{
    internal static class SearchAppendPageRuntime
    {
        internal static Panel Build(SearchAppendPage view, MainForm window)
        {
        bool Confirm(string s) => Ui.Confirm(window, s);
        var ui = DesignerPageRuntime.Attach(view);
        var page = ui.Page;var source = ui.Get<TextBox>("source");var dest = ui.Get<TextBox>("dest");var terms = ui.Get<TextBox>("terms");var ignore = ui.Get<ModernCheckBox>("ignore");var move = ui.Get<ModernCheckBox>("move");var summary = ui.Get<Label>("summary");
        string[] Keywords()=>terms.Text.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries).Where(x=>!string.IsNullOrWhiteSpace(x)).ToArray();
        ui.BindPath("source", "sourcebrowsebutton", "Search file", false, Ui.All, false);
        ui.BindPath("dest", "destbrowsebutton", "Destination", true, Ui.All, false);
        ui.BindAsync("previewMatchesButton", async()=>{var s=source.Text;var k=Keywords();bool ic=ignore.Checked;await window.Run((t,p)=>FileTools.PreviewSearch(s,k,ic,t),r=>summary.Text=$"{r.Matched:N0} matching · {r.Unmatched:N0} nonmatching lines");});
        ui.BindAsync("searchAndAppendButton", async()=>{var s=source.Text;var d=FileTools.Output(dest.Text);var k=Keywords();bool ic=ignore.Checked,m=move.Checked;if(m&&!Confirm("Move matching lines? The source will change. An exact backup will be created."))return;if(File.Exists(d)&&!Confirm("Append matching lines to the existing destination?"))return;await window.Run((t,p)=>FileTools.SearchAppend(s,d,k,ic,m,t,p),r=>summary.Text=$"{(m?"Moved":"Copied")} {r.Matched:N0} lines."+(r.Backup!=null?"\nBackup: "+r.Backup:""));});return page;
        }
    }
}
