using Drizzle.Common.Helpers;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace Drizzle.UI.UWP.Helpers;

public class BrowserUtil : IBrowserUtil
{
    public async Task<bool> OpenBrowserAsync(Uri uri) =>
        await Launcher.LaunchUriAsync(uri);

    public async Task<bool> OpenBrowserAsync(string url)
    {
        try
        {
            var uri = new Uri(url);
            return await Launcher.LaunchUriAsync(uri);
        }
        catch
        {
            return false;
        }
    }
}
