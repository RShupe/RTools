using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using RTools.Core;

namespace RTools.UI.Pages
{
    internal static class FolderAnalyzerPageRuntime
    {
        internal static Panel Build(FolderAnalyzerPage view, MainForm window)
        {
        var ui = DesignerPageRuntime.Attach(view);
        var page = ui.Page;var path = ui.Get<TextBox>("path");var count = ui.Get<ModernNumber>("count");var summary = ui.Get<Label>("summary");var children = ui.Get<DataGridView>("children");var largest = ui.Get<DataGridView>("largest");
        void Open(DataGridView g){if(g.CurrentRow?.DataBoundItem is FolderItem item){var target=Directory.Exists(item.Path)?item.Path:Path.GetDirectoryName(item.Path);if(target!=null&&Directory.Exists(target))System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(target){UseShellExecute=true});}}
        ui.BindPath("path", "pathbrowsebutton", "Folder", false, Ui.All, true);
        ui.BindAsync("analyzeFolderButton", async()=>{var root=path.Text;int max=(int)count.Value;await window.Run((t,p)=>FolderAnalyzer.Scan(root,max,t,p),r=>{summary.Text=$"{FolderAnalyzer.FormatSize(r.Bytes)} · {r.Files:N0} files · {r.Folders:N0} folders · {r.Skipped:N0} skipped";Ui.Bind(children,r.Children);Ui.Bind(largest,r.Largest);});});
        ui.BindClick("copySummaryButton", ()=>Ui.Copy(summary.Text));
        ui.BindClick("openSelectedLocationButton", ()=>Open(children));
        ui.BindClick("openSelectedLocationButton2", ()=>Open(largest));return page;
        }
    }
}
