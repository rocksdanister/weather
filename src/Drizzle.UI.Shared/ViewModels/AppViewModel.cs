using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drizzle.Common.Services;

namespace Drizzle.UI.Shared.ViewModels;

public sealed partial class AppViewModel : ObservableObject
{
    private readonly IWindowService windowService;

    public AppViewModel(IWindowService windowService)
    {
        this.windowService = windowService;   
    }

    [RelayCommand]
    private void OpenAboutWindow()
    {
        windowService.ShowAboutWindow();
    }
}
