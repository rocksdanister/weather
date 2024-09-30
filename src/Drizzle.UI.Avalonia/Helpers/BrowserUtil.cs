using Drizzle.Common.Helpers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Drizzle.UI.Avalonia.Helpers;

public class BrowserUtil : IBrowserUtil
{
    // TODO: Test in Linux?
    // Ref: https://github.com/dotnet/runtime/issues/17938#issuecomment-390472777
    public Task<bool> OpenBrowserAsync(Uri uri)
    {
        try
        {
            var ps = new ProcessStartInfo(uri.AbsoluteUri)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }
        catch
        {
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }

    public Task<bool> OpenBrowserAsync(string url)
    {
        try
        {
            OpenBrowserAsync(new Uri(url));
        }
        catch
        {
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }
}
