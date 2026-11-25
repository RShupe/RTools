using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using RTools.Core;

namespace RTools.UI.Pages
{
    internal static class CsvWorkbenchPageRuntime
    {
        internal static Panel Build(CsvWorkbenchPage view, MainForm window)
        {
        bool Confirm(string s) => Ui.Confirm(window, s);
        bool Overwrite(string path) => Ui.Overwrite(window, path);
        var ui = DesignerPageRuntime.Attach(view);
        var page = ui.Page;var path = ui.Get<TextBox>("path");var output = ui.Get<TextBox>("output");var separator = ui.Get<ModernChoice>("separator");var hasHeader = ui.Get<ModernCheckBox>("hasHeader");var includeHeader = ui.Get<ModernCheckBox>("includeHeader");var safe = ui.Get<ModernCheckBox>("safe");var filter = ui.Get<TextBox>("filter");var column = ui.Get<ModernChoice>("column");var columns = ui.Get<ModernCheckedListBox>("columns");var summary = ui.Get<Label>("summary");var grid = ui.Get<DataGridView>("grid");
        DataTable? table=null;string[] headers=Array.Empty<string>();var sources=new HashSet<string>(FileTools.PathComparer);bool dirty=false,loading=false;
        char Sep()=>separator.SelectedIndex switch{1=>'\t',2=>';',3=>'|',_=>','};
        void Require(){if(table==null)throw new InvalidOperationException("Load a CSV file first.");}
        void Commit(){grid.EndEdit();if(grid.DataSource!=null&&grid.BindingContext[grid.DataSource] is CurrencyManager cm)cm.EndCurrentEdit();}
        void Summary(){if(table!=null)summary.Text=$"{table.DefaultView.Count:N0} visible / {table.Rows.Count:N0} rows · {headers.Length} columns"+(dirty?" · Unsaved workbench edits":"");}
        void Filter(){if(table==null||loading)return;Commit();string term=filter.Text;string escaped=string.Concat(term.Select(c=>c switch{'\''=>"''",'['=>"[[]",']'=>"[]]",'%'=>"[%]",'*'=>"[*]",_=>c.ToString()}));var keys=column.SelectedIndex<=0?Enumerable.Range(0,headers.Length):new[]{column.SelectedIndex-1};table.DefaultView.RowFilter=term.Length==0?"":string.Join(" OR ",keys.Select(i=>$"[C{i}] LIKE '%{escaped}%'"));Summary();}
        void Load(CsvData data,string file)
        {
            loading=true;try
            {
                grid.DataSource=null;grid.Columns.Clear();table?.Dispose();table=new DataTable{CaseSensitive=false,Locale=CultureInfo.InvariantCulture};headers=data.Headers;
                for(int i=0;i<headers.Length;i++){table.Columns.Add("C"+i,typeof(string));Ui.Column(grid,headers[i].Length==0?$"Column {i+1}":headers[i],"C"+i,140);grid.Columns[i].SortMode=DataGridViewColumnSortMode.Automatic;grid.Columns[i].ReadOnly=false;}
                table.BeginLoadData();foreach(var row in data.Rows)table.Rows.Add(row.Cast<object>().ToArray());table.EndLoadData();
                grid.DataSource=table.DefaultView;columns.Items.Clear();column.Items.Clear();column.Items.Add("All columns");foreach(var h in headers){var text=h.Length==0?"(empty header)":h;columns.Items.Add(text,true);column.Items.Add(text);}columns.RefreshExtent();column.SelectedIndex=0;filter.Text="";
                sources.Clear();sources.Add(Path.GetFullPath(file));dirty=false;table.RowChanged+=(_,_)=>{if(!loading){dirty=true;Summary();}};table.RowDeleted+=(_,_)=>{if(!loading){dirty=true;Summary();}};Summary();
            }finally{loading=false;}
        }
        filter.TextChanged+=(_,_)=>{try{Filter();}catch(Exception ex){Ui.Error(window,ex);}};column.SelectedIndexChanged+=(_,_)=>{try{Filter();}catch(Exception ex){Ui.Error(window,ex);}};
        ui.BindAsync("load", async()=>{Commit();if(dirty&&!Confirm("Discard the current workbench edits and load a new file?"))return;var src=path.Text;var sep=Sep();bool header=hasHeader.Checked;await window.Run((t,p)=>CsvWorkbench.Read(src,sep,header,t),data=>Load(data,src));});
        ui.BindAsync("merge", async()=>{Require();Commit();using var d=new OpenFileDialog{Filter=Ui.Csv};if(d.ShowDialog(window)!=DialogResult.OK)return;string src=d.FileName;var sep=Sep();bool header=hasHeader.Checked;await window.Run((t,p)=>CsvWorkbench.Read(src,sep,header,t),data=>{if(!data.Headers.SequenceEqual(headers))throw new InvalidDataException("Column headers and order must match exactly before merging.");CsvWorkbench.ValidateSize(table!.Rows.Count+data.Rows.Count,headers.Length);loading=true;table.BeginLoadData();try{foreach(var row in data.Rows)table.Rows.Add(row.Cast<object>().ToArray());}finally{table.EndLoadData();loading=false;}sources.Add(Path.GetFullPath(src));dirty=true;Summary();});});
        ui.BindClick("add", ()=>{Require();Commit();CsvWorkbench.ValidateSize(table!.Rows.Count+1,headers.Length);table.Rows.Add(Enumerable.Repeat<object>("",headers.Length).ToArray());Summary();});
        ui.BindClick("remove", ()=>{Require();Commit();var selected=grid.SelectedCells.Cast<DataGridViewCell>().Select(c=>grid.Rows[c.RowIndex].DataBoundItem).OfType<DataRowView>().Select(x=>x.Row).Distinct().ToArray();if(selected.Length==0||!Confirm($"Remove {selected.Length} rows from the workbench? Source files are unchanged."))return;foreach(var row in selected)table!.Rows.Remove(row);Summary();});
        ui.BindClick("dedup", ()=>{Require();Commit();var seen=new HashSet<string>(StringComparer.Ordinal);var duplicates=new List<DataRow>();foreach(DataRow row in table!.Rows){string key=string.Concat(row.ItemArray.Select(v=>{string s=Convert.ToString(v,CultureInfo.InvariantCulture)??"";return s.Length.ToString(CultureInfo.InvariantCulture)+":"+s;}));if(!seen.Add(key))duplicates.Add(row);}if(duplicates.Count==0){summary.Text="No exact duplicate rows.";return;}if(!Confirm($"Remove {duplicates.Count:N0} exact duplicate rows from the full table?"))return;loading=true;try{foreach(var row in duplicates)table.Rows.Remove(row);}finally{loading=false;}dirty=true;Summary();});
        ui.BindAsync("export", async()=>{Require();Commit();var dst=FileTools.Output(output.Text);foreach(var s in sources)FileTools.EnsureDifferent(s,dst);var indexes=columns.CheckedIndices.Cast<int>().ToArray();if(indexes.Length==0)throw new ArgumentException("Select at least one export column.");if(!Overwrite(dst))return;var names=indexes.Select(i=>headers[i]).ToArray();var rows=table!.DefaultView.Cast<DataRowView>().Select(r=>indexes.Select(i=>Convert.ToString(r[i],CultureInfo.InvariantCulture)??"").ToArray()).ToArray();var sep=Sep();bool header=includeHeader.Checked,sanitize=safe.Checked;await window.Run((t,p)=>{CsvWorkbench.Write(dst,names,rows,sep,header,sanitize,t);return rows.Length;},n=>summary.Text=$"Exported {n:N0} visible rows to {dst}. Source files unchanged.");});
        ui.BindPath("path", "pathbrowsebutton", "Source file", false, Ui.Csv, false);
        ui.BindPath("output", "outputbrowsebutton", "Export destination", true, Ui.Csv, false);return page;
        }
    }
}
