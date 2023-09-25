using Drizzle.Common;
using Drizzle.UI.UWP.AnimatedVisuals;
using Drizzle.UI.UWP.Helpers;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Drizzle.UI.UWP.Converters;

public class WmoWeatherToAnimatedVisualConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return icons[(WmoWeatherCode)value];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    // Reuse the same icon across the app
    private static readonly IReadOnlyDictionary<WmoWeatherCode, IAnimatedVisualSource> icons = new Dictionary<WmoWeatherCode, IAnimatedVisualSource>()
    {
        { WmoWeatherCode.ClearSky, new ClearDay() },
        { WmoWeatherCode.MainlyClear, new Cloudy() },
        { WmoWeatherCode.PartlyCloudy, new Cloudy() },
        { WmoWeatherCode.Overcast, new Overcast() },
        { WmoWeatherCode.Fog, new Fog() },
        { WmoWeatherCode.DepositingRimeFog, new Fog() },
        { WmoWeatherCode.LightDrizzle, new AnimatedVisuals.Drizzle() },
        { WmoWeatherCode.ModerateDrizzle, new AnimatedVisuals.Drizzle() },
        { WmoWeatherCode.DenseDrizzle, new DrizzleExtreme() },
        { WmoWeatherCode.LightFreezingDrizzle, new Snow() },
        { WmoWeatherCode.DenseFreezingDrizzle, new Snow() },
        { WmoWeatherCode.SlightRain, new AnimatedVisuals.Drizzle() },
        { WmoWeatherCode.ModerateRain, new Rain() },
        { WmoWeatherCode.HeavyRain, new RainExtreme()},
        { WmoWeatherCode.LightFreezingRain, new Sleet() },
        { WmoWeatherCode.HeavyFreezingRain, new SleetExtreme() },
        { WmoWeatherCode.SlightSnowFall, new Snow() },
        { WmoWeatherCode.ModerateSnowFall, new Snow() },
        { WmoWeatherCode.HeavySnowFall, new SnowExtreme() },
        { WmoWeatherCode.SnowGrains, new Hail() },
        { WmoWeatherCode.SlightRainShowers, new AnimatedVisuals.Drizzle() },
        { WmoWeatherCode.ModerateRainShowers, new Rain() },
        { WmoWeatherCode.ViolentRainShowers, new RainExtreme() },
        { WmoWeatherCode.SlightSnowShowers, new Snow() },
        { WmoWeatherCode.HeavySnowShowers, new SnowExtreme() },
        { WmoWeatherCode.Thunderstorm, new Thunderstorms() },
        { WmoWeatherCode.ThunderstormLightHail, new Thunderstorms() },
        { WmoWeatherCode.ThunderstormHeavyHail, new ThunderstormsExtreme() },
    };

    // Create new each time incase icons are made customizable
    private static IAnimatedVisualSource GetIcon(WmoWeatherCode code)
    {
        return code switch
        {
            WmoWeatherCode.ClearSky => new ClearDay(),
            WmoWeatherCode.MainlyClear => new Cloudy(),
            WmoWeatherCode.PartlyCloudy => new Cloudy(),
            WmoWeatherCode.Overcast => new Overcast(),
            WmoWeatherCode.Fog => new Fog(),
            WmoWeatherCode.DepositingRimeFog => new Fog(),
            WmoWeatherCode.LightDrizzle => new AnimatedVisuals.Drizzle(),
            WmoWeatherCode.ModerateDrizzle => new AnimatedVisuals.Drizzle(),
            WmoWeatherCode.DenseDrizzle => new DrizzleExtreme(),
            WmoWeatherCode.LightFreezingDrizzle => new Snow(),
            WmoWeatherCode.DenseFreezingDrizzle => new Snow(),
            WmoWeatherCode.SlightRain => new AnimatedVisuals.Drizzle(),
            WmoWeatherCode.ModerateRain => new Rain(),
            WmoWeatherCode.HeavyRain => new RainExtreme(),
            WmoWeatherCode.LightFreezingRain => new Sleet(),
            WmoWeatherCode.HeavyFreezingRain => new SleetExtreme(),
            WmoWeatherCode.SlightSnowFall => new Snow(),
            WmoWeatherCode.ModerateSnowFall => new Snow(),
            WmoWeatherCode.HeavySnowFall => new SnowExtreme(),
            WmoWeatherCode.SnowGrains => new Hail(),
            WmoWeatherCode.SlightRainShowers => new AnimatedVisuals.Drizzle(),
            WmoWeatherCode.ModerateRainShowers => new Rain(),
            WmoWeatherCode.ViolentRainShowers => new RainExtreme(),
            WmoWeatherCode.SlightSnowShowers => new Snow(),
            WmoWeatherCode.HeavySnowShowers => new SnowExtreme(),
            WmoWeatherCode.Thunderstorm => new Thunderstorms(),
            WmoWeatherCode.ThunderstormLightHail => new Thunderstorms(),
            WmoWeatherCode.ThunderstormHeavyHail => new ThunderstormsExtreme(),
            _ => throw new NotImplementedException()
        };
    }
}
