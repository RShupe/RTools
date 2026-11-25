using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using RTools.Core;

namespace RTools.UI.Pages
{
    internal static class FileComparisonPageRuntime
    {
        internal static Panel Build(FileComparisonPage view, MainForm window)
        {
        var ui = DesignerPageRuntime.Attach(view);
        var page = ui.Page;var a = ui.Get<TextBox>("a");var b = ui.Get<TextBox>("b");var mode = ui.Get<ModernChoice>("mode");var ic = ui.Get<ModernCheckBox>("ic");var iw = ui.Get<ModernCheckBox>("iw");var only = ui.Get<ModernCheckBox>("only");var summary = ui.Get<Label>("summary");var grid = ui.Get<DataGridView>("grid");DiffResult? last=null;
        grid.CellFormatting+=(_,e)=>{if(e.RowIndex<0)return;if(grid.Rows[e.RowIndex].DataBoundItem is DiffRow row){if((e.ColumnIndex==1||e.ColumnIndex==0)&&(row.Kind=="Removed"||row.Kind=="Changed"))e.CellStyle.BackColor=Theme.Removed;else if((e.ColumnIndex==2||e.ColumnIndex==3)&&(row.Kind=="Added"||row.Kind=="Changed"))e.CellStyle.BackColor=Theme.Added;}};
        void Render(){if(last==null)grid.DataSource=null;else Ui.Bind(grid,only.Checked?last.Rows.Where(r=>r.Kind!="Equal"):last.Rows);}
        only.CheckedChanged+=(_,_)=>Render();mode.SelectedIndexChanged+=(_,_)=>{ic.Enabled=iw.Enabled=only.Enabled=mode.SelectedIndex==0;};
        ui.BindPath("a", "abrowsebutton", "Original file", false, Ui.All, false);
        ui.BindPath("b", "bbrowsebutton", "Modified file", false, Ui.All, false);
        ui.BindAsync("compareFilesButton", async()=>{var left=a.Text;var right=b.Text;var ignoreCase=ic.Checked;var ignoreSpace=iw.Checked;last=null;Render();if(mode.SelectedIndex==1)await window.Run((t,p)=>Comparison.Binary(left,right,t),r=>summary.Text=(r.FirstDifference==null?"IDENTICAL":"DIFFERENT")+$" · {r.LeftBytes:N0} / {r.RightBytes:N0} bytes"+(r.FirstDifference is long off?$" · First differing byte: {off:N0} (0x{off:X})":""));else await window.Run((t,p)=>Comparison.Text(left,right,ignoreCase,ignoreSpace,t),r=>{last=r;summary.Text=$"{r.Added:N0} added · {r.Removed:N0} removed · {r.Changed:N0} changed · {r.Rows.Count-r.Added-r.Removed-r.Changed:N0} equal";Render();});});
        ui.BindClick("copySummaryButton", ()=>Ui.Copy(summary.Text));return page;
        }
    }
}
