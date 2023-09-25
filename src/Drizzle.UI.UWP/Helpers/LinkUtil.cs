using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace Drizzle.UI.UWP.Helpers
{
    public static class LinkUtil
    {
        public static async Task<bool> OpenBrowser(Uri uri) =>
            await Launcher.LaunchUriAsync(uri);

        public static async Task<bool> OpenBrowser(string url)
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
}
