using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using RTools.Core;

namespace RTools.UI.Pages
{
    internal static class RegexTesterPageRuntime
    {
        internal static Panel Build(RegexTesterPage view, MainForm window)
        {
        var ui = DesignerPageRuntime.Attach(view);
        var page = ui.Page;var pattern = ui.Get<TextBox>("pattern");var sample = ui.Get<RichTextBox>("sample");var replace = ui.Get<TextBox>("replace");var preview = ui.Get<ModernCheckBox>("preview");var ic = ui.Get<ModernCheckBox>("ic");var multi = ui.Get<ModernCheckBox>("multi");var single = ui.Get<ModernCheckBox>("single");var result = ui.Get<RichTextBox>("result");var summary = ui.Get<Label>("summary");var grid = ui.Get<DataGridView>("grid");bool highlighting=false;
        void Clear(){if(highlighting)return;grid.DataSource=null;result.Text="";summary.Text="Run the expression to refresh the results.";}
        sample.TextChanged+=(_,_)=>Clear();pattern.TextChanged+=(_,_)=>Clear();replace.TextChanged+=(_,_)=>Clear();foreach(var c in new[]{ic,multi,single,preview})c.CheckedChanged+=(_,_)=>Clear();
        void Highlight(string text,IReadOnlyList<RegexHit> hits){highlighting=true;try{int selection=sample.SelectionStart;sample.SelectAll();sample.SelectionBackColor=Theme.Input;sample.SelectionColor=Theme.Ink;foreach(var hit in hits){if(hit.Length==0)continue;sample.Select(hit.Index,hit.Length);sample.SelectionBackColor=Theme.Highlight;sample.SelectionColor=Theme.SelectedText;}sample.Select(Math.Min(selection,sample.TextLength),0);}finally{highlighting=false;}}
        ui.BindAsync("runRegexButton", async()=>{string text=sample.Text,expr=pattern.Text,rep=replace.Text;RegexOptions opts=RegexOptions.None;if(ic.Checked)opts|=RegexOptions.IgnoreCase;if(multi.Checked)opts|=RegexOptions.Multiline;if(single.Checked)opts|=RegexOptions.Singleline;bool replacing=preview.Checked;await window.Run((t,p)=>RegexWorkbench.Run(text,expr,rep,opts,replacing,t),r=>{Ui.Bind(grid,r.Hits);Highlight(text,r.Hits);summary.Text=$"{r.Hits.Count:N0} matches"+(r.Truncated?" (limit reached; replacement suppressed)":"");result.Text=r.Replacement??"";});});
        ui.BindClick("copyReplacementButton", ()=>Ui.Copy(result.Text));return page;
        }
    }
}
