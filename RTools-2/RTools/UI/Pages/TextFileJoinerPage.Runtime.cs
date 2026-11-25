using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using RTools.Core;

namespace RTools.UI.Pages
{
    internal static class TextFileJoinerPageRuntime
    {
        internal static Panel Build(TextFileJoinerPage view, MainForm window)
        {
        bool Confirm(string s) => Ui.Confirm(window, s);
        bool Overwrite(string path) => Ui.Overwrite(window, path);
        string[] Paths(ListBox list) => list.Items.Cast<string>().ToArray();
        var ui = DesignerPageRuntime.Attach(view);
        var page = ui.Page;var files = ui.Get<ModernListBox>("files");var output = ui.Get<TextBox>("output");var spaces = ui.Get<ModernNumber>("spaces");var delete = ui.Get<ModernCheckBox>("delete");var summary = ui.Get<Label>("summary");
        ui.BindFiles("files", ()=>summary.Text=$"{files.Items.Count:N0} files selected", "addFilesButton", "removeSelectedButton", "clearbutton", "moveUpButton", "moveDownButton");
        ui.BindPath("output", "outputbrowsebutton", "Output file", true, Ui.All, false);
        ui.BindAsync("joinFilesButton", async()=>{var input=Paths(files);var dest=FileTools.Output(output.Text);var count=(int)spaces.Value;bool remove=delete.Checked;if(input.Length==0)throw new ArgumentException("Add at least one file.");if(!Overwrite(dest)||remove&&!Confirm($"Delete {input.Length} originals after the output has been saved?"))return;await window.Run((t,p)=>FileTools.Join(input,dest,count,remove,t,p),r=>summary.Text=$"Joined {r.Files:N0} files ({r.Characters:N0} characters)."+(r.DeleteFailures.Count==0?"":"\nSome originals could not be deleted:\n"+string.Join("\n",r.DeleteFailures)));});return page;
        }
    }
}
