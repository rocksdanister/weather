using Avalonia;
using Avalonia.Data.Converters;
using Drizzle.Models.Weather;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Drizzle.UI.Avalonia.Converters;

public class WmoWeatherToAnimatedVisualConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is int code && values[1] is bool isDayTime)
            return isDayTime ? dayIcons[(WmoWeatherCode)code] : nightIcons[(WmoWeatherCode)code];
        return null;
    }

    private static readonly IReadOnlyDictionary<WmoWeatherCode, string> dayIcons = new Dictionary<WmoWeatherCode, string>
    {
        { WmoWeatherCode.ClearSky, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/ClearDay.json" },
        { WmoWeatherCode.MainlyClear, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Cloudy.json" },
        { WmoWeatherCode.PartlyCloudy, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/PartlyCloudyDay.json" },
        { WmoWeatherCode.Overcast, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Overcast.json" },
        { WmoWeatherCode.Haze, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Haze.json" },
        { WmoWeatherCode.Dust, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/DustDay.json" },
        { WmoWeatherCode.Mist, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Mist.json" },
        { WmoWeatherCode.Fog, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Fog.json" },
        { WmoWeatherCode.DepositingRimeFog, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Fog.json" },
        { WmoWeatherCode.LightDrizzle, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Drizzle.json" },
        { WmoWeatherCode.ModerateDrizzle, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Drizzle.json" },
        { WmoWeatherCode.DenseDrizzle, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/DrizzleExtreme.json" },
        { WmoWeatherCode.LightFreezingDrizzle, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Snow.json" },
        { WmoWeatherCode.DenseFreezingDrizzle, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Snow.json" },
        { WmoWeatherCode.SlightRain, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Drizzle.json" },
        { WmoWeatherCode.ModerateRain, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Rain.json" },
        { WmoWeatherCode.HeavyRain, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/RainExtreme.json" },
        { WmoWeatherCode.LightFreezingRain, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Sleet.json" },
        { WmoWeatherCode.HeavyFreezingRain, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/SleetExtreme.json" },
        { WmoWeatherCode.SlightSnowFall, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Snow.json" },
        { WmoWeatherCode.ModerateSnowFall, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Snow.json" },
        { WmoWeatherCode.HeavySnowFall, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/SnowExtreme.json" },
        { WmoWeatherCode.SnowGrains, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Hail.json" },
        { WmoWeatherCode.SlightRainShowers, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Drizzle.json" },
        { WmoWeatherCode.ModerateRainShowers, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Rain.json" },
        { WmoWeatherCode.ViolentRainShowers, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/RainExtreme.json" },
        { WmoWeatherCode.SlightSnowShowers, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Snow.json" },
        { WmoWeatherCode.HeavySnowShowers, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/SnowExtreme.json" },
        { WmoWeatherCode.Thunderstorm, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Thunderstorms.json" },
        { WmoWeatherCode.ThunderstormLightHail, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Thunderstorms.json" },
        { WmoWeatherCode.ThunderstormHeavyHail, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/ThunderstormsExtreme.json" }
    };

    private static readonly IReadOnlyDictionary<WmoWeatherCode, string> nightIcons = new Dictionary<WmoWeatherCode, string>
    {
        { WmoWeatherCode.ClearSky, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/ClearNight.json" },
        { WmoWeatherCode.MainlyClear, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Cloudy.json" },
        { WmoWeatherCode.PartlyCloudy, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/PartlyCloudyNight.json" },
        { WmoWeatherCode.Overcast, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Overcast.json" },
        { WmoWeatherCode.Haze, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Haze.json" },
        { WmoWeatherCode.Dust, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/DustNight.json" },
        { WmoWeatherCode.Mist, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Mist.json" },
        { WmoWeatherCode.Fog, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Fog.json" },
        { WmoWeatherCode.DepositingRimeFog, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Fog.json" },
        { WmoWeatherCode.LightDrizzle, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Drizzle.json" },
        { WmoWeatherCode.ModerateDrizzle, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Drizzle.json" },
        { WmoWeatherCode.DenseDrizzle, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/DrizzleExtreme.json" },
        { WmoWeatherCode.LightFreezingDrizzle, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Snow.json" },
        { WmoWeatherCode.DenseFreezingDrizzle, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Snow.json" },
        { WmoWeatherCode.SlightRain, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Drizzle.json" },
        { WmoWeatherCode.ModerateRain, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Rain.json" },
        { WmoWeatherCode.HeavyRain, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/RainExtreme.json" },
        { WmoWeatherCode.LightFreezingRain, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Sleet.json" },
        { WmoWeatherCode.HeavyFreezingRain, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/SleetExtreme.json" },
        { WmoWeatherCode.SlightSnowFall, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Snow.json" },
        { WmoWeatherCode.ModerateSnowFall, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Snow.json" },
        { WmoWeatherCode.HeavySnowFall, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/SnowExtreme.json" },
        { WmoWeatherCode.SnowGrains, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Hail.json" },
        { WmoWeatherCode.SlightRainShowers, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Drizzle.json" },
        { WmoWeatherCode.ModerateRainShowers, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Rain.json" },
        { WmoWeatherCode.ViolentRainShowers, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/RainExtreme.json" },
        { WmoWeatherCode.SlightSnowShowers, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Snow.json" },
        { WmoWeatherCode.HeavySnowShowers, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/SnowExtreme.json" },
        { WmoWeatherCode.Thunderstorm, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Thunderstorms.json" },
        { WmoWeatherCode.ThunderstormLightHail, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/Thunderstorms.json" },
        { WmoWeatherCode.ThunderstormHeavyHail, "avares://Drizzle.UI.Avalonia/Assets/WeatherIcons/ThunderstormsExtreme.json" }
    };
}
