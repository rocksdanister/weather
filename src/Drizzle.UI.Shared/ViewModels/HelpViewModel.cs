using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drizzle.Common.Helpers;
using System.Threading.Tasks;

namespace Drizzle.UI.Shared.ViewModels;

public partial class HelpViewModel : ObservableObject
{
    private readonly IBrowserUtil browserUtil;

    public HelpViewModel(IBrowserUtil browserUtil)
    {
        this.browserUtil = browserUtil;
    }

    [RelayCommand]
    private async Task OpenWebsite()
    {
        await browserUtil.OpenBrowserAsync("https://www.rocksdanister.com/weather");
    }

    [RelayCommand]
    private async Task OpenSource()
    {
        await browserUtil.OpenBrowserAsync("https://github.com/rocksdanister/weather");
    }

    [RelayCommand]
    private async Task OpenContact()
    {
        await browserUtil.OpenBrowserAsync("https://github.com/rocksdanister/weather/wiki/Frequently-Asked-Questions-(FAQ)");
    }
}
