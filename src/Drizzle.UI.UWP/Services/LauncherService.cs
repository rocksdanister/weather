using Drizzle.Common.Services;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace Drizzle.UI.UWP.Services;

public class LauncherService : ILauncherService
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

    public async Task<bool> OpenFileAsync(string filePath)
    {
        try
        {
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            return await Launcher.LaunchFileAsync(file);
        }
        catch
        {
            return false;
        }
    }
}
