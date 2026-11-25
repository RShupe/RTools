using System.Net.Http;
using System.Diagnostics;

namespace RTools;

public static class UpdateChecker
{
    public const string Repository="https://github.com/RShupe/RTools";
    public const string Releases=Repository+"/releases";
    public const string VersionFile="https://github.com/RShupe/RTools/raw/main/currentreleaseversion.txt";
    private static readonly HttpClient Client=new HttpClient { Timeout=TimeSpan.FromSeconds(10), MaxResponseContentBufferSize=4096 };
    public static Version Parse(string content)
    {
        var text=content.Trim().TrimStart('\uFEFF').Trim();if(text.StartsWith("v",StringComparison.OrdinalIgnoreCase))text=text.Substring(1);
        if(!Version.TryParse(text,out var version))throw new InvalidDataException("The GitHub version file must contain a numeric version such as 2.2.0.0.");return version;
    }
    private static Version Normalize(Version version)=>new Version(version.Major,version.Minor,Math.Max(0,version.Build),Math.Max(0,version.Revision));
    public static bool IsNewer(Version published,Version installed)=>Normalize(published).CompareTo(Normalize(installed))>0;
    public static async Task<Version> Fetch(CancellationToken token)
    {
        using var request=new HttpRequestMessage(HttpMethod.Get,VersionFile);request.Headers.UserAgent.ParseAdd("RTools-Updater/"+MainForm.DisplayVersion);request.Headers.CacheControl=new System.Net.Http.Headers.CacheControlHeaderValue { NoCache=true };
        using var response=await Client.SendAsync(request,token).ConfigureAwait(false);response.EnsureSuccessStatusCode();return Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
    }
    public static void Open(string url)=>Process.Start(new ProcessStartInfo(url){UseShellExecute=true});
}
