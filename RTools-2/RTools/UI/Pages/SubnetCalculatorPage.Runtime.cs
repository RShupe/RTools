using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using RTools.Core;

namespace RTools.UI.Pages
{
    internal static class SubnetCalculatorPageRuntime
    {
        internal static Panel Build(SubnetCalculatorPage view, MainForm window)
        {
        var ui = DesignerPageRuntime.Attach(view);
        var page = ui.Page;var address = ui.Get<TextBox>("address");var mask = ui.Get<TextBox>("mask");var result = ui.Get<RichTextBox>("result");
        void Calculate(){var s=SubnetCalculator.Calculate(address.Text,mask.Text);result.Text=$"Address             {s.Address}/{s.Prefix}\nSubnet mask         {s.Mask}\nWildcard mask       {s.Wildcard}\nNetwork address     {s.Network}\nBroadcast address   {s.Broadcast}\nFirst usable host   {s.FirstHost}\nLast usable host    {s.LastHost}\nTotal addresses     {s.Total:N0}\nUsable addresses    {s.Usable:N0}\nBinary mask         {s.Binary}"+(s.Prefix==31?"\n\n/31: point-to-point host pair (RFC 3021).":s.Prefix==32?"\n\n/32: single host route.":"");}
        ui.BindClick("calculatebutton", Calculate);
        ui.BindClick("copyResultsButton", ()=>Ui.Copy(result.Text));Calculate();return page;
        }
    }
}
