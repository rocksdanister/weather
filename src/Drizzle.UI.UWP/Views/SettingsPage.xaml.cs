using Drizzle.UI.UWP.Extensions;
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
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.IsPresetUnit) && !vm.IsPresetUnit && !WeatherUnitControl.IsExpanded)
                {
                    WeatherUnitControl.IsExpanded = true;
                    SettingsScrollViewer.ScrollToElement(WeatherUnitControl);
                }
            };
        }
    }
}
