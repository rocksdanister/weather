using Drizzle.Models;
using Drizzle.Models.Enums;
using Drizzle.Models.Weather;
using System;
using System.Collections.Generic;

namespace Drizzle.Common.Constants;

public static class UserSettingsConstants
{
    public const string Performance = "Performance";

    public const string WeatherUnit = "WeatherUnit";

    public const string Theme = "AppTheme";

    public const string AutoHideScreensaverMenu = "AutoHideScreensaverMenu";

    public const string IncludeUserImagesInShuffle = "IncludeUserImagesInShuffle";

    public const string OpenWeatherMapKey = "OpenWeatherMapKey";

    public const string QweatherApiKey = "QweatherApiKey";

    public const string PinnedLocations = "PinnedLocations";

    public const string SelectedLocation = "SelectedLocation";

    public const string MaxPinnedLocations = "MaxPinnedLocations";

    public const string CacheWeather = "CacheWeather";

    public const string BackgroundBrightness = "BackgroundBrightness";

    public const string ReducedMotion = "ReducedMotion";

    public const string BackgroundPause = "BackgroundPause";

    public const string BackgroundPauseAudio = "BackgroundPauseAudio";

    public const string SoundVolume = "SoundVolume";

    public const string SelectedWeatherProvider = "SelectedWeatherProvider";

    // Custom user selected units.

    public const string SelectedTemperatureUnit = "SelectedTemperatureUnit";

    public const string SelectedWindSpeedUnit = "SelectedWindSpeedUnit";

    public const string SelectedVisibilityUnit = "SelectedVisibilityUnit";

    public const string SelectedPressureUnit = "SelectedPressureUnit";

    public const string SelectedPrecipitationUnit = "SelectedPrecipitationUnit";

    public const string SelectedMainGraphType = "SelectedMainGraphType";

    public static IReadOnlyDictionary<string, object> Defaults { get; } = new Dictionary<string, object>()
    {
        { Performance, AppPerformance.performance },
        { WeatherUnit, WeatherUnits.metric },
        { Theme, AppTheme.dark },
        { AutoHideScreensaverMenu, false },
        { IncludeUserImagesInShuffle, false },
        { OpenWeatherMapKey, string.Empty },
        { QweatherApiKey, string.Empty },
        { PinnedLocations, Array.Empty<LocationModel>() },
        { SelectedLocation, null },
        { MaxPinnedLocations, 5 },
        { CacheWeather, true },
        { BackgroundBrightness, 1f },
        { ReducedMotion, false },
        { BackgroundPause, true },
        { BackgroundPauseAudio, true },
        { SoundVolume, 0 },
        { SelectedWeatherProvider, WeatherProviders.OpenMeteo },
        { SelectedTemperatureUnit, TemperatureUnits.degree },
        { SelectedWindSpeedUnit, WindSpeedUnits.kmh },
        { SelectedVisibilityUnit, VisibilityUnits.km },
        { SelectedPressureUnit, PressureUnits.hPa_mb },
        { SelectedMainGraphType, GraphType.temperature },
        { SelectedPrecipitationUnit, PrecipitationUnits.mm }
    };
}
