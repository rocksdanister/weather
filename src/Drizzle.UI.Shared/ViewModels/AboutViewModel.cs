using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Drizzle.Common.Helpers;
using Drizzle.Common.Services;



#if WINDOWS_UWP
using Drizzle.UI.UWP.Helpers;
#else
using System.Reflection;
#endif

namespace Drizzle.UI.Shared.ViewModels;

public sealed partial class AboutViewModel : ObservableObject
{
    private readonly IBrowserUtil browserUtil;

    public AboutViewModel(IBrowserUtil browserUtil, ISystemInfoProvider systemInfo)
    {
        this.browserUtil = browserUtil;

        AppVersion = $"v{systemInfo.AppVersion}";
    }

    [ObservableProperty]
    private string appVersion;

    [RelayCommand]
    private async Task OpenPersonalWebsite()
    {
        await browserUtil.OpenBrowserAsync("https://rocksdanister.com");
    }

    [RelayCommand]
    private async Task OpenGithub()
    {
        await browserUtil.OpenBrowserAsync("https://github.com/rocksdanister");
    }

    [RelayCommand]
    private async Task OpenTwitter()
    {
        await browserUtil.OpenBrowserAsync("https://twitter.com/rocksdanister");
    }

    [RelayCommand]
    private async Task OpenYoutube()
    {
        await browserUtil.OpenBrowserAsync("https://www.youtube.com/channel/UClep84ofxC41H8-R9UfNPSQ");
    }

    [RelayCommand]
    private async Task OpenReddit()
    {
        await browserUtil.OpenBrowserAsync("https://reddit.com/u/rocksdanister");
    }

    [RelayCommand]
    private async Task OpenEmail()
    {
        await browserUtil.OpenBrowserAsync("mailto:awoo.git@gmail.com");
    }
}
