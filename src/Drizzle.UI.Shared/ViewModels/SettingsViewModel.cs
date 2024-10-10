using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drizzle.Common.Constants;
using Drizzle.Common.Services;
using Drizzle.Models;
using Drizzle.Models.Enums;
using Drizzle.Models.Weather;
using Drizzle.UI.Shared.Factories;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Drizzle.UI.Shared.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IUserSettings userSettings;
    private readonly IWeatherClientFactory weatherClientFactory;
    private readonly IFileService fileService;
    private readonly ILauncherService launcher;
    private readonly IResourceService resources;

    public SettingsViewModel(IUserSettings userSettings,
        ShellViewModel shellVm,
        IWeatherClientFactory weatherClientFactory,
        IFileService fileService,
        ILauncherService launcher,
        IResourceService resources)
    {
        this.weatherClientFactory = weatherClientFactory;
        this.userSettings = userSettings;
        this.ShellVm = shellVm;
        this.fileService = fileService;
        this.launcher = launcher;
        this.resources = resources;

        ReducedMotion = userSettings.Get<bool>(UserSettingsConstants.ReducedMotion);
        BackgroundPause = userSettings.Get<bool>(UserSettingsConstants.BackgroundPause);
        BackgroundPauseAudio = userSettings.Get<bool>(UserSettingsConstants.BackgroundPauseAudio);
        BackgroundBrightness = userSettings.Get<float>(UserSettingsConstants.BackgroundBrightness);
        Custombackground = userSettings.Get<bool>(UserSettingsConstants.IncludeUserImagesInShuffle);
        SelectedAppThemeIndex = (int)userSettings.GetAndDeserialize<AppTheme>(UserSettingsConstants.Theme);
        SelectedWeatherUnitIndex = (int)userSettings.GetAndDeserialize<WeatherUnits>(UserSettingsConstants.WeatherUnit);
        SelectedAppPerformanceIndex = (int)userSettings.GetAndDeserialize<AppPerformance>(UserSettingsConstants.Performance);
        SelectedWeatherProviderIndex = (int)userSettings.GetAndDeserialize<WeatherProviders>(UserSettingsConstants.SelectedWeatherProvider);

        // Ref: https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
        Languages = [
            new() { DisplayName = resources.GetString("LanguageSettingsItemSystemDefault/Content"), Code = string.Empty },
            new() { DisplayName = "English", Code = "en-US" },
            new() { DisplayName = "عربى", Code = "ar-AE" }, // Arabic (United Arab Emirates)
            new() { DisplayName = "Български", Code = "bg-BG" }, // Bulgarian
            new() { DisplayName = "Deutsch", Code = "de-DE" }, // German
            new() { DisplayName = "Ελληνικά", Code = "el-GR" }, // Greek
            new() { DisplayName = "Español", Code = "es-ES" }, // Spanish
            new() { DisplayName = "فارسی", Code = "fa-IR" }, // Persian
            new() { DisplayName = "Suomi", Code = "fi-FI" }, // Finnish
            new() { DisplayName = "Français", Code = "fr-FR" }, // French
            new() { DisplayName = "עברית", Code = "he-IL" }, // Hebrew
            new() { DisplayName = "Magyar", Code = "hu-HU" }, // Hungarian
            new() { DisplayName = "Bahasa Indonesia", Code = "id-ID" }, // Indonesian
            new() { DisplayName = "Italiano", Code = "it-IT" }, // Italian
            new() { DisplayName = "日本語", Code = "ja-JP" }, // Japanese
            new() { DisplayName = "한국어", Code = "ko-KR" }, // Korean
            new() { DisplayName = "Bahasa Melayu", Code = "ms-MY" }, // Malay
            new() { DisplayName = "Nederlands", Code = "nl-NL" }, // Dutch
            new() { DisplayName = "Norsk", Code = "no-NO" }, // Norwegian
            new() { DisplayName = "Polski", Code = "pl-PL" }, // Polish
            new() { DisplayName = "Português (Brasil)", Code = "pt-BR" }, // Portuguese (Brazil)
            new() { DisplayName = "Português (Portugal)", Code = "pt-PT" }, // Portuguese (Portugal)
            new() { DisplayName = "Română", Code = "ro-RO" }, // Romanian
            new() { DisplayName = "Русский", Code = "ru-RU" }, // Russian
            new() { DisplayName = "Slovensky", Code = "sk-SK" }, // Slovak
            new() { DisplayName = "Svenska", Code = "sv-SE" }, // Swedish
            new() { DisplayName = "Türkçe", Code = "tr-TR" }, // Turkish
            new() { DisplayName = "Українська", Code = "uk-UA" }, // Ukrainian
            new() { DisplayName = "Tiếng Việt", Code = "vi-VN" }, // Vietnamese
            new() { DisplayName = "中文", Code = "zh-CN" }, // Chinese (Simplified)
            new() { DisplayName = "中文 (繁體)", Code = "zh-Hant" } // Chinese (Traditional)
        ];
        SelectedLanguage = userSettings.Get<bool>(UserSettingsConstants.UseSystemDefaultLanguage) ? 
            Languages[0] : Languages.FirstOrDefault(x => x.Code == userSettings.Get<string>(UserSettingsConstants.SelectedLanguageCode)) ?? Languages[0];
    }

    public ShellViewModel ShellVm { get; }

    public ObservableCollection<LanguageModel> Languages { get; }

    [ObservableProperty]
    private bool isLanguageChanged;

    private LanguageModel _selectedLanguage;
    public LanguageModel SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (value == Languages[0])
            {
                if (!userSettings.Get<bool>(UserSettingsConstants.UseSystemDefaultLanguage))
                    userSettings.Set(UserSettingsConstants.UseSystemDefaultLanguage, true);

                if (!string.IsNullOrEmpty(userSettings.Get<string>(UserSettingsConstants.SelectedLanguageCode)))
                {
                    userSettings.Set(UserSettingsConstants.SelectedLanguageCode, string.Empty);
                    IsLanguageChanged = true;
                }
            }
            else if (value != null)
            {
                if (userSettings.Get<bool>(UserSettingsConstants.UseSystemDefaultLanguage))
                    userSettings.Set(UserSettingsConstants.UseSystemDefaultLanguage, false);

                if (userSettings.Get<string>(UserSettingsConstants.SelectedLanguageCode) != value.Code)
                {
                    userSettings.Set(UserSettingsConstants.SelectedLanguageCode, value.Code);
                    IsLanguageChanged = true;
                }
            }
            SetProperty(ref _selectedLanguage, value);
        }
    }

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
        if (!Directory.Exists(fileService.LogFolderPath))
            return;

        // DateCreated is not reliably available in some file system/OS combinations.
        var latestLog = Directory.GetFiles(fileService.LogFolderPath).OrderByDescending(f => new FileInfo(f).LastWriteTime).FirstOrDefault();
        if (latestLog != null)
            await launcher.OpenFileAsync(latestLog);
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
            ShellVm.ErrorMessage = resources.GetString("StringWeatherKeyMissingRestoredDefault");
        }
        else
        {
            // If provider updated, save state.
            if (currentWeatherProvider != newWeatherProvider)
                userSettings.SetAndSerialize(UserSettingsConstants.SelectedWeatherProvider, SelectedWeatherProviderIndex);
        }
    }

    private static string? GetWeatherProviderApiSettingsKey(WeatherProviders provider)
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
