using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drizzle.Common.Services;
using System.Threading.Tasks;

namespace Drizzle.UI.Shared.ViewModels;

public partial class HelpViewModel : ObservableObject
{
    private readonly ILauncherService launcher;

    public HelpViewModel(ILauncherService launcher)
    {
        this.launcher = launcher;
    }

    [RelayCommand]
    private async Task OpenWebsite()
    {
        await launcher.OpenBrowserAsync("https://www.rocksdanister.com/weather");
    }

    [RelayCommand]
    private async Task OpenSource()
    {
        await launcher.OpenBrowserAsync("https://github.com/rocksdanister/weather");
    }

    [RelayCommand]
    private async Task OpenContact()
    {
        await launcher.OpenBrowserAsync("https://github.com/rocksdanister/weather/wiki/Frequently-Asked-Questions-(FAQ)");
    }
}
