using Drizzle.UI.UWP.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Drizzle.UI.UWP.Views
{
    public sealed partial class SettingsPage : Page
    {
        private readonly SettingsViewModel viewModel;

        public SettingsPage(SettingsViewModel vm)
        {
            this.InitializeComponent();
            this.viewModel = vm;
            this.DataContext = vm;

            // Only open when custom unit selected, do nothing for preset units.
            WeatherUnitControl.IsExpanded = !vm.IsPresetUnit;
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.IsPresetUnit) && !vm.IsPresetUnit)
                    WeatherUnitControl.IsExpanded = true;
            };
        }
    }
}
