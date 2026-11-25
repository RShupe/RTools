using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using RTools.Core;

namespace RTools.UI.Pages
{
    internal static class LogViewerPageRuntime
    {
        internal static Panel Build(LogViewerPage view, MainForm window)
        {
        string Pick(ModernChoice box) => box.SelectedItem?.ToString() ?? "";
        var ui = DesignerPageRuntime.Attach(view);
        var page = ui.Page;var path = ui.Get<TextBox>("path");var keyword = ui.Get<TextBox>("keyword");var severity = ui.Get<ModernChoice>("severity");var count = ui.Get<ModernNumber>("count");var ignore = ui.Get<ModernCheckBox>("ignore");var follow = ui.Get<ModernCheckBox>("follow");var summary = ui.Get<Label>("summary");var grid = ui.Get<DataGridView>("grid");
        var timer=new System.Windows.Forms.Timer{Interval=2000};bool running=false;string? lastStamp=null;CancellationTokenSource? current=null;
        void Render(LogSnapshot r){Ui.Bind(grid,r.Lines);summary.Text=$"{r.Lines.Count:N0} displayed · {r.Scanned:N0} lines scanned · {FolderAnalyzer.FormatSize(r.Bytes)}"+(r.Partial?" · Last 1 MiB only":"");if(grid.Rows.Count>0&&follow.Checked)grid.FirstDisplayedScrollingRowIndex=grid.Rows.Count-1;}
        grid.CellFormatting+=(_,e)=>{if(e.RowIndex<0)return;if(grid.Rows[e.RowIndex].DataBoundItem is LogLine l){if(l.Severity=="Error")e.CellStyle.BackColor=Theme.Removed;else if(l.Severity=="Warning")e.CellStyle.BackColor=Theme.Warning;}};
        void Reset(){lastStamp=null;current?.Cancel();}
        foreach(var box in new[]{path,keyword})box.TextChanged+=(_,_)=>Reset();severity.SelectedIndexChanged+=(_,_)=>Reset();count.ValueChanged+=(_,_)=>Reset();ignore.CheckedChanged+=(_,_)=>Reset();
        timer.Tick+=async (_,_)=>
        {
            if(running||window.Busy||!page.Visible||!follow.Checked)return;running=true;using var cts=CancellationTokenSource.CreateLinkedTokenSource(window.Lifetime);current=cts;
            try
            {
                string src=path.Text,k=keyword.Text,sev=Pick(severity);int limit=(int)count.Value;bool ic=ignore.Checked;string? old=lastStamp;
                var snapshot=await Task.Run(()=>{var info=new FileInfo(FileTools.Input(src));string stamp=src+"|"+info.Length+"|"+info.LastWriteTimeUtc.Ticks+"|"+info.CreationTimeUtc.Ticks;return (Stamp:stamp,Data:stamp==old?null:LogReader.Read(src,k,sev,limit,ic,cts.Token));},cts.Token);
                if(!cts.IsCancellationRequested&&page.Visible&&follow.Checked&&!window.Busy){lastStamp=snapshot.Stamp;if(snapshot.Data!=null)Render(snapshot.Data);}
            }catch(OperationCanceledException){}catch(Exception ex){if(!page.IsDisposed&&!cts.IsCancellationRequested){summary.Text="Follow stopped: "+ex.Message;follow.Checked=false;}}
            finally{current=null;running=false;}
        };
        follow.CheckedChanged+=(_,_)=>{Reset();timer.Enabled=follow.Checked&&page.Visible;};page.VisibleChanged+=(_,_)=>{timer.Enabled=page.Visible&&follow.Checked;if(!page.Visible)Reset();};page.Disposed+=(_,_)=>{current?.Cancel();timer.Stop();timer.Dispose();};
        ui.BindPath("path", "pathbrowsebutton", "Log file", false, Ui.All, false);
        ui.BindAsync("readLogButton", async()=>{follow.Checked=false;Reset();var src=path.Text;var k=keyword.Text;var sev=Pick(severity);int n=(int)count.Value;bool ic=ignore.Checked;await window.Run((t,p)=>LogReader.Read(src,k,sev,n,ic,t),Render);});return page;
        }
    }
}
