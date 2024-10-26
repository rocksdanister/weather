using Avalonia.Controls;
using Drizzle.UI.Shared.ViewModels;

namespace Drizzle.UI.Avalonia.Views;

public partial class SettingsView : UserControl
{
    public SettingsView(SettingsViewModel vm)
    {
        InitializeComponent();
        this.DataContext = vm;

        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.IsPresetUnit) && !vm.IsPresetUnit && !WeatherUnitControl.IsExpanded)
            {
                WeatherUnitControl.IsExpanded = true;
            }
        };
    }
}