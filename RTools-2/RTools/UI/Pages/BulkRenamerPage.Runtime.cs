using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using RTools.Core;

namespace RTools.UI.Pages
{
    internal static class BulkRenamerPageRuntime
    {
        internal static Panel Build(BulkRenamerPage view, MainForm window)
        {
        bool Confirm(string s) => Ui.Confirm(window, s);
        string Pick(ModernChoice box) => box.SelectedItem?.ToString() ?? "";
        string[] Paths(ListBox list) => list.Items.Cast<string>().ToArray();
        var ui = DesignerPageRuntime.Attach(view);
        var page = ui.Page;var list = ui.Get<ModernListBox>("list");var find = ui.Get<TextBox>("find");var replacement = ui.Get<TextBox>("replacement");var prefix = ui.Get<TextBox>("prefix");var suffix = ui.Get<TextBox>("suffix");var regex = ui.Get<ModernCheckBox>("regex");var ignore = ui.Get<ModernCheckBox>("ignore");var number = ui.Get<ModernCheckBox>("number");var start = ui.Get<ModernNumber>("start");var digits = ui.Get<ModernNumber>("digits");var casing = ui.Get<ModernChoice>("casing");var summary = ui.Get<Label>("summary");var grid = ui.Get<DataGridView>("grid");IReadOnlyList<RenameItem>? plan=null,undo=null;Button? apply=null,undoButton=null;
        void Invalidate(){plan=null;grid.DataSource=null;summary.Text=$"{list.Items.Count:N0} files · preview required";if(apply!=null)apply.Enabled=false;}
        void ReplaceList(IEnumerable<string> paths){list.Items.Clear();Ui.AddFiles(list,paths);Invalidate();}
        foreach(var control in new[]{find,replacement,prefix,suffix})control.TextChanged+=(_,_)=>Invalidate();foreach(var control in new[]{regex,ignore,number})control.CheckedChanged+=(_,_)=>Invalidate();start.ValueChanged+=(_,_)=>Invalidate();digits.ValueChanged+=(_,_)=>Invalidate();casing.SelectedIndexChanged+=(_,_)=>Invalidate();
        apply = ui.Get<Button>("apply"); ui.BindAsync("apply", async()=>{var reviewed=plan;if(reviewed==null||!Confirm($"Rename {reviewed.Count(x=>x.Status=="Ready"):N0} files? No existing destinations will be overwritten."))return;await window.Run((t,p)=>BulkRename.Apply(reviewed,t),r=>{undo=r;undoButton!.Enabled=true;ReplaceList(reviewed.Select(x=>x.Destination));summary.Text=$"Renamed {r.Count:N0} files. Undo is available for the last batch.";});});apply.Enabled=false;
        undoButton = ui.Get<Button>("undoButton"); ui.BindAsync("undoButton", async()=>{var reviewed=undo;if(reviewed==null||!Confirm("Restore the previous filenames?"))return;await window.Run((t,p)=>BulkRename.Apply(reviewed,t),r=>{var unchanged=Paths(list).Where(x=>!reviewed.Any(y=>FileTools.PathComparer.Equals(x,y.Source))).ToArray();ReplaceList(unchanged.Concat(reviewed.Select(x=>x.Destination)));undo=null;undoButton!.Enabled=false;summary.Text=$"Restored {r.Count:N0} filenames.";});});undoButton.Enabled=false;
        ui.BindFiles("list", Invalidate, "addFilesButton", "removeSelectedButton", "clearbutton");
        ui.BindAsync("previewbutton", async()=>{var paths=Paths(list);var opts=new RenameOptions(find.Text,replacement.Text,regex.Checked,ignore.Checked,prefix.Text,suffix.Text,number.Checked,(int)start.Value,(int)digits.Value,Pick(casing));Invalidate();await window.Run((t,p)=>BulkRename.Preview(paths,opts,t),r=>{plan=r;Ui.Bind(grid,r);int ready=r.Count(x=>x.Status=="Ready"),unchanged=r.Count(x=>x.Status=="Unchanged");summary.Text=$"{ready:N0} changes · {unchanged:N0} unchanged · {r.Count-ready-unchanged:N0} blocked";apply.Enabled=ready>0&&ready+unchanged==r.Count;});});return page;
        }
    }
}
