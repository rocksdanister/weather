using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drizzle.Common.Services;
using System.Threading.Tasks;

namespace Drizzle.UI.Shared.ViewModels;

public sealed partial class AboutViewModel : ObservableObject
{
    private readonly ILauncherService launcher;

    public AboutViewModel(ILauncherService launcher, ISystemInfoProvider systemInfo)
    {
        this.launcher = launcher;
        AppVersion = $"v{systemInfo.AppVersion}";
    }

    [ObservableProperty]
    private string appVersion;

    [RelayCommand]
    private async Task OpenPersonalWebsite()
    {
        await launcher.OpenBrowserAsync("https://rocksdanister.com");
    }

    [RelayCommand]
    private async Task OpenGithub()
    {
        await launcher.OpenBrowserAsync("https://github.com/rocksdanister");
    }

    [RelayCommand]
    private async Task OpenTwitter()
    {
        await launcher.OpenBrowserAsync("https://twitter.com/rocksdanister");
    }

    [RelayCommand]
    private async Task OpenYoutube()
    {
        await launcher.OpenBrowserAsync("https://www.youtube.com/channel/UClep84ofxC41H8-R9UfNPSQ");
    }

    [RelayCommand]
    private async Task OpenReddit()
    {
        await launcher.OpenBrowserAsync("https://reddit.com/u/rocksdanister");
    }

    [RelayCommand]
    private async Task OpenEmail()
    {
        await launcher.OpenBrowserAsync("mailto:awoo.git@gmail.com");
    }
}
