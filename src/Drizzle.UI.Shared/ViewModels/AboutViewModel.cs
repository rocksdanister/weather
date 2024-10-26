using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drizzle.Common.Services;
using Drizzle.Models.Enums;
using System.Threading.Tasks;

namespace Drizzle.UI.Shared.ViewModels;

public sealed partial class AboutViewModel : ObservableObject
{
    private readonly IResourceService resources;
    private readonly IAppUpdaterService appUpdater;
    private readonly ILauncherService launcher;

    public AboutViewModel(ILauncherService launcher,
        ISystemInfoProvider systemInfo,
        IResourceService resources,
        IAppUpdaterService appUpdater)
    {
        this.resources = resources;
        this.launcher = launcher;
        this.appUpdater = appUpdater;

        AppVersion = $"v{systemInfo.AppVersion}";
        UpdateStatus(appUpdater.LastCheckedStatus);
        appUpdater.UpdateChecked += AppUpdater_UpdateChecked;
    }

    [ObservableProperty]
    private string appVersion;

    [ObservableProperty]
    private AppNotification updateNotificationType;

    [ObservableProperty]
    private string updateNotificationMessage;

    [ObservableProperty]
    private bool isUpdateAvailable;

    [RelayCommand]
    private async Task CheckUpdate()
    {
        await appUpdater.CheckUpdateAsync();
    }

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

    private void AppUpdater_UpdateChecked(object sender, AppUpdateStatus e)
    {
        UpdateStatus(e);
    }

    private void UpdateStatus(AppUpdateStatus status)
    {
        switch (status)
        {
            case AppUpdateStatus.uptodate:
                {
                    UpdateNotificationType = AppNotification.information;
                    UpdateNotificationMessage = $"{resources.GetString("RefreshWeather/Text")} {appUpdater.LastCheckedTime.ToLocalTime()}";
                }
                break;
            case AppUpdateStatus.available:
                {
                    UpdateNotificationType = AppNotification.success;
                    UpdateNotificationMessage = resources.GetString("TitleUpdateAvailable/Text");
                }
                break;
            case AppUpdateStatus.invalid:
                {
                    UpdateNotificationType = AppNotification.warning;
                    UpdateNotificationMessage = "Invalid~";
                }
                break;
            case AppUpdateStatus.error:
                {
                    UpdateNotificationType = AppNotification.error;
                    UpdateNotificationMessage = resources.GetString("InfoError/Title");
                }
                break;
            case AppUpdateStatus.notchecked:
                {
                    UpdateNotificationType = AppNotification.information;
                    // Don't show anything.
                    UpdateNotificationMessage = null;
                }
                break;
        }
        IsUpdateAvailable = status == AppUpdateStatus.available;
    }
}
