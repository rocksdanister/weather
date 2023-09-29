using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drizzle.Common;
using Drizzle.Common.Constants;
using Drizzle.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace Drizzle.UI.UWP.ViewModels
{
    public sealed partial class SettingsViewModel : ObservableObject
    {
        private readonly IUserSettings userSettings;

        public SettingsViewModel(IUserSettings userSettings, ShellViewModel shellVm)
        {
            this.userSettings = userSettings;

            Custombackground = userSettings.Get<bool>(UserSettingsConstants.IncludeUserImagesInShuffle);
            BackgroundBrightness = userSettings.Get<float>(UserSettingsConstants.BackgroundBrightness);
            SelectedAppPerformanceIndex = (int)userSettings.GetAndDeserialize<AppPerformance>(UserSettingsConstants.Performance);
            SelectedWeatherUnitIndex = (int)userSettings.GetAndDeserialize<UserWeatherUnits>(UserSettingsConstants.WeatherUnit);
            SelectedAppThemeIndex = (int)userSettings.GetAndDeserialize<AppTheme>(UserSettingsConstants.Theme);

            IsHardwareAccelerated = shellVm.IsHardwareAccelerated;
        }

        [ObservableProperty]
        private bool isHardwareAccelerated = true;

        private bool _customBackground;
        public bool Custombackground
        {
            get => _customBackground;
            set
            {
                if (userSettings.Get<bool>(UserSettingsConstants.IncludeUserImagesInShuffle) != value)
                    userSettings.Set(UserSettingsConstants.IncludeUserImagesInShuffle, value);

                SetProperty(ref _customBackground, value);
            }
        }

        private int _selectedAppThemeIndex;
        public int SelectedAppThemeIndex
        {
            get => _selectedAppThemeIndex;
            set
            {
                if (userSettings.GetAndDeserialize<AppTheme>(UserSettingsConstants.Theme) != (AppTheme)value)
                    userSettings.SetAndSerialize(UserSettingsConstants.Theme, value);

                SetProperty(ref _selectedAppThemeIndex, value);
            }
        }

        private int _selectedAppPerformanceIndex;
        public int SelectedAppPerformanceIndex
        {
            get => _selectedAppPerformanceIndex;
            set
            {
                if (userSettings.GetAndDeserialize<AppPerformance>(UserSettingsConstants.Performance) != (AppPerformance)value)
                    userSettings.SetAndSerialize(UserSettingsConstants.Performance, value);

                SetProperty(ref _selectedAppPerformanceIndex, value);
            }
        }

        private int _selectedWeatherUnitIndex;
        public int SelectedWeatherUnitIndex
        {
            get => _selectedWeatherUnitIndex;
            set
            {
                if (userSettings.GetAndDeserialize<UserWeatherUnits>(UserSettingsConstants.WeatherUnit) != (UserWeatherUnits)value)
                    userSettings.SetAndSerialize(UserSettingsConstants.WeatherUnit, value);

                SetProperty(ref _selectedWeatherUnitIndex, value);
            }
        }

        private float _backgroundBrightness;
        public float BackgroundBrightness
        {
            get => _backgroundBrightness;
            set
            {
                if (userSettings.Get<float>(UserSettingsConstants.BackgroundBrightness) != value)
                    userSettings.Set(UserSettingsConstants.BackgroundBrightness, value);

                SetProperty(ref _backgroundBrightness, value);
            }
        }

        private RelayCommand _openLogsCommand;
        public RelayCommand OpenLogsCommand => _openLogsCommand ??= new RelayCommand(async() => {
            var localFolder = ApplicationData.Current.LocalFolder;
            var logsFolder = await localFolder.GetFolderAsync("Logs");
            var sortedLogs = (await logsFolder.GetFilesAsync()).OrderByDescending(f => f.DateCreated);
            var latestLog = sortedLogs.FirstOrDefault();

            if (latestLog != null)
                await Launcher.LaunchFileAsync(latestLog);
        });
    }
}
