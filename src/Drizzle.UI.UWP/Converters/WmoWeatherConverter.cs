using Drizzle.Common;
using Drizzle.Models.Weather;
using Drizzle.UI.UWP.AnimatedVisuals;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;

namespace Drizzle.UI.UWP.Converters
{
    public static class WmoWeatherConverter
    {
        public static IAnimatedVisualSource WmoWeatherToAnimatedVisual(int code, bool isDayTime)
        {
            return isDayTime ? dayIcons[(WmoWeatherCode)code] : nightIcons[(WmoWeatherCode)code];
        }

        public static IAnimatedVisualSource WmoWeatherToAnimatedVisual(int code)
        {
            return WmoWeatherToAnimatedVisual(code, true);
        }

        public static string WmoWeatherToFluentIcon(int code)
        {
            return $"ms-appx:///Assets/Weather/Fluent Icons/{code}d.svg";
        }

        public static string WmoWeatherToString(int code)
        {
            try
            {
                return resourceLoader?.GetString($"WmoWeatherString{code}");
            }
            catch
            {
                return "Error";
            }
        }

        private static readonly ResourceLoader resourceLoader =
            CoreWindow.GetForCurrentThread() is not null ? ResourceLoader.GetForCurrentView() : null;

        // Reuse the same icon across the app
        private static readonly IReadOnlyDictionary<WmoWeatherCode, IAnimatedVisualSource> dayIcons = new Dictionary<WmoWeatherCode, IAnimatedVisualSource>()
        {
            { WmoWeatherCode.ClearSky, new ClearDay() },
            { WmoWeatherCode.MainlyClear, new Cloudy() },
            { WmoWeatherCode.PartlyCloudy, new PartlyCloudyDay() },
            { WmoWeatherCode.Overcast, new Overcast() },
            { WmoWeatherCode.Haze, new Haze() },
            { WmoWeatherCode.Dust, new DustDay() },
            { WmoWeatherCode.Mist, new Mist() },
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

        private static readonly IReadOnlyDictionary<WmoWeatherCode, IAnimatedVisualSource> nightIcons = new Dictionary<WmoWeatherCode, IAnimatedVisualSource>()
        {
            { WmoWeatherCode.ClearSky, new ClearNight() },
            { WmoWeatherCode.MainlyClear, new Cloudy() },
            { WmoWeatherCode.PartlyCloudy, new PartlyCloudyNight() },
            { WmoWeatherCode.Overcast, new Overcast() },
            { WmoWeatherCode.Haze, new Haze() },
            { WmoWeatherCode.Dust, new DustNight() },
            { WmoWeatherCode.Mist, new Mist() },
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
        //private static IAnimatedVisualSource GetDayIcon(WmoWeatherCode code)
        //{
        //    return code switch
        //    {
        //        WmoWeatherCode.ClearSky => new ClearDay(),
        //        WmoWeatherCode.MainlyClear => new Cloudy(),
        //        WmoWeatherCode.PartlyCloudy => new Cloudy(),
        //        WmoWeatherCode.Overcast => new Overcast(),
        //        WmoWeatherCode.Haze => new Haze(),
        //        WmoWeatherCode.Dust => new DustDay(),
        //        WmoWeatherCode.Mist => new Mist(),
        //        WmoWeatherCode.Fog => new Fog(),
        //        WmoWeatherCode.DepositingRimeFog => new Fog(),
        //        WmoWeatherCode.LightDrizzle => new AnimatedVisuals.Drizzle(),
        //        WmoWeatherCode.ModerateDrizzle => new AnimatedVisuals.Drizzle(),
        //        WmoWeatherCode.DenseDrizzle => new DrizzleExtreme(),
        //        WmoWeatherCode.LightFreezingDrizzle => new Snow(),
        //        WmoWeatherCode.DenseFreezingDrizzle => new Snow(),
        //        WmoWeatherCode.SlightRain => new AnimatedVisuals.Drizzle(),
        //        WmoWeatherCode.ModerateRain => new Rain(),
        //        WmoWeatherCode.HeavyRain => new RainExtreme(),
        //        WmoWeatherCode.LightFreezingRain => new Sleet(),
        //        WmoWeatherCode.HeavyFreezingRain => new SleetExtreme(),
        //        WmoWeatherCode.SlightSnowFall => new Snow(),
        //        WmoWeatherCode.ModerateSnowFall => new Snow(),
        //        WmoWeatherCode.HeavySnowFall => new SnowExtreme(),
        //        WmoWeatherCode.SnowGrains => new Hail(),
        //        WmoWeatherCode.SlightRainShowers => new AnimatedVisuals.Drizzle(),
        //        WmoWeatherCode.ModerateRainShowers => new Rain(),
        //        WmoWeatherCode.ViolentRainShowers => new RainExtreme(),
        //        WmoWeatherCode.SlightSnowShowers => new Snow(),
        //        WmoWeatherCode.HeavySnowShowers => new SnowExtreme(),
        //        WmoWeatherCode.Thunderstorm => new Thunderstorms(),
        //        WmoWeatherCode.ThunderstormLightHail => new Thunderstorms(),
        //        WmoWeatherCode.ThunderstormHeavyHail => new ThunderstormsExtreme(),
        //        _ => throw new NotImplementedException()
        //    };
        //}
    }
}
