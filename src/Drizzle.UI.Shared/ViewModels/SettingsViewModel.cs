using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drizzle.Common.Constants;
using Drizzle.Common.Services;
using Drizzle.Models.Enums;
using Drizzle.Models.Weather;
using Drizzle.UI.UWP.Factories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System;

namespace Drizzle.UI.UWP.ViewModels
{
    public sealed partial class SettingsViewModel : ObservableObject
    {
        private readonly IUserSettings userSettings;
        private readonly IWeatherClientFactory weatherClientFactory;
        private readonly ResourceLoader resourceLoader;

        public SettingsViewModel(IUserSettings userSettings, ShellViewModel shellVm, IWeatherClientFactory weatherClientFactory)
        {
            this.weatherClientFactory = weatherClientFactory;
            this.userSettings = userSettings;
            this.ShellVm = shellVm;

            if (Windows.UI.Core.CoreWindow.GetForCurrentThread() is not null)
                resourceLoader = ResourceLoader.GetForCurrentView();

            ReducedMotion = userSettings.Get<bool>(UserSettingsConstants.ReducedMotion);
            BackgroundPause = userSettings.Get<bool>(UserSettingsConstants.BackgroundPause);
            BackgroundPauseAudio = userSettings.Get<bool>(UserSettingsConstants.BackgroundPauseAudio);
            BackgroundBrightness = userSettings.Get<float>(UserSettingsConstants.BackgroundBrightness);
            Custombackground = userSettings.Get<bool>(UserSettingsConstants.IncludeUserImagesInShuffle);
            SelectedAppThemeIndex = (int)userSettings.GetAndDeserialize<AppTheme>(UserSettingsConstants.Theme);
            SelectedWeatherUnitIndex = (int)userSettings.GetAndDeserialize<WeatherUnits>(UserSettingsConstants.WeatherUnit);
            SelectedAppPerformanceIndex = (int)userSettings.GetAndDeserialize<AppPerformance>(UserSettingsConstants.Performance);
            SelectedWeatherProviderIndex = (int)userSettings.GetAndDeserialize<WeatherProviders>(UserSettingsConstants.SelectedWeatherProvider);
        }

        public ShellViewModel ShellVm { get; }

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

        private bool _backgroundPause;
        public bool BackgroundPause
        {
            get => _backgroundPause;
            set
            {
                if (userSettings.Get<bool>(UserSettingsConstants.BackgroundPause) != value)
                    userSettings.Set(UserSettingsConstants.BackgroundPause, value);

                SetProperty(ref _backgroundPause, value);
            }
        }

        private bool _backgroundPauseAudio;
        public bool BackgroundPauseAudio
        {
            get => _backgroundPauseAudio;
            set
            {
                if (userSettings.Get<bool>(UserSettingsConstants.BackgroundPauseAudio) != value)
                    userSettings.Set(UserSettingsConstants.BackgroundPauseAudio, value);

                SetProperty(ref _backgroundPauseAudio, value);
            }
        }

        private int _selectedWeatherUnitIndex;
        public int SelectedWeatherUnitIndex
        {
            get => _selectedWeatherUnitIndex;
            set
            {
                SetProperty(ref _selectedWeatherUnitIndex, value);
                IsPresetUnit = (WeatherUnits)value != WeatherUnits.custom;
                switch ((WeatherUnits)value)
                {
                    case WeatherUnits.metric:
                    case WeatherUnits.imperial:
                    case WeatherUnits.hybrid:
                        var units = new WeatherUnitSettings((WeatherUnits)value);
                        SelectedTemperatureUnitIndex = (int)units.TemperatureUnit;
                        SelectedWindSpeedUnitIndex = (int)units.WindSpeedUnit;
                        SelectedVisibilityUnitIndex = (int)units.VisibilityUnit;
                        SelectedPrecipitationUnitIndex = (int)units.PrecipitationUnit;
                        break;
                    case WeatherUnits.custom:
                        SelectedTemperatureUnitIndex = (int)userSettings.GetAndDeserialize<TemperatureUnits>(UserSettingsConstants.SelectedTemperatureUnit);
                        SelectedWindSpeedUnitIndex = (int)userSettings.GetAndDeserialize<WindSpeedUnits>(UserSettingsConstants.SelectedWindSpeedUnit);
                        SelectedVisibilityUnitIndex = (int)userSettings.GetAndDeserialize<VisibilityUnits>(UserSettingsConstants.SelectedVisibilityUnit);
                        SelectedPrecipitationUnitIndex = (int)userSettings.GetAndDeserialize<PrecipitationUnits>(UserSettingsConstants.SelectedPrecipitationUnit);
                        break;
                }
            }
        }

        private bool customWeatherUnitValueChanged;

        private int _selectedTemperatureUnitIndex;
        public int SelectedTemperatureUnitIndex
        {
            get => _selectedTemperatureUnitIndex;
            set
            {
                if ((WeatherUnits)SelectedWeatherUnitIndex == WeatherUnits.custom && userSettings.GetAndDeserialize<TemperatureUnits>(UserSettingsConstants.SelectedTemperatureUnit) != (TemperatureUnits)value)
                {
                    customWeatherUnitValueChanged = true;
                    userSettings.SetAndSerialize(UserSettingsConstants.SelectedTemperatureUnit, value);
                }
                SetProperty(ref _selectedTemperatureUnitIndex, value);
            }
        }

        private int _selectedWindSpeedUnitIndex;
        public int SelectedWindSpeedUnitIndex
        {
            get => _selectedWindSpeedUnitIndex;
            set
            {
                if ((WeatherUnits)SelectedWeatherUnitIndex == WeatherUnits.custom && userSettings.GetAndDeserialize<WindSpeedUnits>(UserSettingsConstants.SelectedWindSpeedUnit) != (WindSpeedUnits)value)
                {
                    customWeatherUnitValueChanged = true;
                    userSettings.SetAndSerialize(UserSettingsConstants.SelectedWindSpeedUnit, value);
                }
                SetProperty(ref _selectedWindSpeedUnitIndex, value);
            }
        }

        private int _selectedVisibilityUnitIndex;
        public int SelectedVisibilityUnitIndex
        {
            get => _selectedVisibilityUnitIndex;
            set
            {
                if ((WeatherUnits)SelectedWeatherUnitIndex == WeatherUnits.custom && userSettings.GetAndDeserialize<VisibilityUnits>(UserSettingsConstants.SelectedVisibilityUnit) != (VisibilityUnits)value)
                {
                    customWeatherUnitValueChanged = true;
                    userSettings.SetAndSerialize(UserSettingsConstants.SelectedVisibilityUnit, value);
                }
                SetProperty (ref _selectedVisibilityUnitIndex, value);
            }
        }

        private int _selectedPrecipitationUnitIndex;
        public int SelectedPrecipitationUnitIndex
        {
            get => _selectedPrecipitationUnitIndex;
            set
            {
                if ((WeatherUnits)SelectedWeatherUnitIndex == WeatherUnits.custom && userSettings.GetAndDeserialize<PrecipitationUnits>(UserSettingsConstants.SelectedPrecipitationUnit) != (PrecipitationUnits)value)
                {
                    customWeatherUnitValueChanged = true;
                    userSettings.SetAndSerialize(UserSettingsConstants.SelectedPrecipitationUnit, value);
                }
                SetProperty(ref _selectedPrecipitationUnitIndex, value);
            }
        }

        private int _selectedWeatherProviderIndex;
        public int SelectedWeatherProviderIndex
        {
            get => _selectedWeatherProviderIndex;
            set
            {
                SetProperty(ref _selectedWeatherProviderIndex, value);
                var settingsKey = GetWeatherProviderApiSettingsKey((WeatherProviders)value);
                IsApiKeyRequired = settingsKey != null;
                SelectedApiKey = settingsKey != null ? userSettings.Get<string>(settingsKey) : "...";
            }
        }


        [ObservableProperty]
        private bool isPresetUnit;

        [ObservableProperty]
        private bool isApiKeyRequired;

        private string _selectedApiKey;
        public string SelectedApiKey
        {
            get => _selectedApiKey;
            set
            {
                var weatherProvider = (WeatherProviders)SelectedWeatherProviderIndex;
                var settingsKey = GetWeatherProviderApiSettingsKey(weatherProvider);
                if (settingsKey != null && userSettings.Get<string>(settingsKey) != value)
                {
                    userSettings.Set(settingsKey, value);
                    weatherClientFactory.GetInstance(weatherProvider).ApiKey = value;
                }
                SetProperty(ref _selectedApiKey, value);
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

        private bool _reducedMotion;
        public bool ReducedMotion
        {
            get => _reducedMotion;
            set
            {
                if (userSettings.Get<bool>(UserSettingsConstants.ReducedMotion) != value)
                    userSettings.Set(UserSettingsConstants.ReducedMotion, value);

                SetProperty(ref _reducedMotion, value);
            }
        }

        [RelayCommand]
        private async Task OpenLogs()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var logsFolder = await localFolder.GetFolderAsync("Logs");
            var sortedLogs = (await logsFolder.GetFilesAsync()).OrderByDescending(f => f.DateCreated);
            var latestLog = sortedLogs.FirstOrDefault();

            if (latestLog != null)
                await Launcher.LaunchFileAsync(latestLog);
        }

        // Save settings that require confirmation here
        public void OnClose()
        {
            // Update Weather units
            var newWeatherUnit = (WeatherUnits)SelectedWeatherUnitIndex;
            var currentWeatherUnit = userSettings.GetAndDeserialize<WeatherUnits>(UserSettingsConstants.WeatherUnit);
            if (newWeatherUnit != currentWeatherUnit || (newWeatherUnit == WeatherUnits.custom && customWeatherUnitValueChanged))
                userSettings.SetAndSerialize(UserSettingsConstants.WeatherUnit, newWeatherUnit);

            // Update Weather provider
            var newWeatherProvider = (WeatherProviders)SelectedWeatherProviderIndex;
            var currentWeatherProvider = userSettings.GetAndDeserialize<WeatherProviders>(UserSettingsConstants.SelectedWeatherProvider);
            var newWeatherProvidersettingsKey = GetWeatherProviderApiSettingsKey(newWeatherProvider);
            // If the newly selected provider require key and its missing.
            if (newWeatherProvidersettingsKey != null && string.IsNullOrWhiteSpace(userSettings.Get<string>(newWeatherProvidersettingsKey)))
            {
                // If key is removed from already selected provider, restore default.
                if (currentWeatherProvider == newWeatherProvider)
                    userSettings.SetAndSerialize(UserSettingsConstants.SelectedWeatherProvider, WeatherProviders.OpenMeteo);

                ShellVm.ErrorMessage = resourceLoader?.GetString("StringWeatherKeyMissingRestoredDefault");
            }
            else
            {
                // If provider updated, save state.
                if (currentWeatherProvider != newWeatherProvider)
                    userSettings.SetAndSerialize(UserSettingsConstants.SelectedWeatherProvider, SelectedWeatherProviderIndex);
            }
        }

        private static string GetWeatherProviderApiSettingsKey(WeatherProviders provider)
        {
            return provider switch
            {
                WeatherProviders.OpenMeteo => null,
                WeatherProviders.OpenWeatherMap => UserSettingsConstants.OpenWeatherMapKey,
                WeatherProviders.Qweather => UserSettingsConstants.QweatherApiKey,
                _ => null,
            };
        }
    }
}
