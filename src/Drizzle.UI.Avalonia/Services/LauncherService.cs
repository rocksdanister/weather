using Drizzle.Common.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Drizzle.UI.Avalonia.Services
{
    public class LauncherService : ILauncherService
    {
        // Note: Not tested in Linux.
        // Ref: https://github.com/dotnet/runtime/issues/17938#issuecomment-390472777
        public Task<bool> OpenFileAsync(string filePath)
        {
            try
            {
                var ps = new ProcessStartInfo(filePath)
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

        public Task<bool> OpenBrowserAsync(Uri uri)
        {
            return OpenFileAsync(uri.AbsoluteUri);
        }

        public Task<bool> OpenBrowserAsync(string url)
        {
            try
            {
                var uri = new Uri(url);
                OpenBrowserAsync(uri);
            }
            catch
            {
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }
    }
}
