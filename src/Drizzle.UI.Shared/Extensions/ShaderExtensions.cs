using Drizzle.Common.Helpers;
using Drizzle.Models.Enums;
using Drizzle.Models.Shaders;
using Drizzle.Models.Shaders.Uniform;
using Drizzle.Models.Weather;
using System;
using System.Diagnostics;
using System.Numerics;

namespace Drizzle.UI.Shared.Extensions;

public static class ShaderExtensions
{
    /// <summary>
    /// Updates src value to dest for the current frame, increment value by small step % every frame if required.
    /// </summary>
    public static void UniformFrameUpdate(this ShaderModel src, ShaderModel dest)
    {
        foreach (var item in src.UniformMappings)
        {
            switch (item.Value.UniformType)
            {
                case UniformType.int_:
                    {
                        var value = (int)item.Value.GetValue(src);
                        item.Value.SetValue(dest, value);
                    }
                    break;
                case UniformType.float_:
                    {
                        var property = item.Value as FloatProperty;
                        if (property.LerpSpeed != 0)
                        {
                            var srcValue = (float)item.Value.GetValue(src);
                            var destValue = (float)item.Value.GetValue(dest);
                            var lerpedValue = MathUtil.Lerp(destValue, srcValue, property.LerpSpeed);
                            item.Value.SetValue(dest, lerpedValue);
                        }
                        else
                        {
                            var value = (float)item.Value.GetValue(src);
                            item.Value.SetValue(dest, value);
                        }
                    }
                    break;
                case UniformType.bool_:
                    {
                        var value = (bool)item.Value.GetValue(src);
                        item.Value.SetValue(dest, value);
                    }
                    break;
                case UniformType.color:
                    {
                        var value = (Vector3)item.Value.GetValue(src);
                        item.Value.SetValue(dest, value);
                    }
                    break;
                case UniformType.textureUri:
                    {
                        var value = (string)item.Value.GetValue(src);
                        item.Value.SetValue(dest, value);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public static ShaderTypes GetShader(this WmoWeatherCode weatherCode)
    {
        return weatherCode switch
        {
            WmoWeatherCode.ClearSky => ShaderTypes.depth,
            WmoWeatherCode.MainlyClear => ShaderTypes.depth,
#if WINDOWS_UWP
            WmoWeatherCode.PartlyCloudy => ShaderTypes.clouds,
            WmoWeatherCode.Overcast => ShaderTypes.clouds,
#else
            WmoWeatherCode.PartlyCloudy => ShaderTypes.depth,
            WmoWeatherCode.Overcast => ShaderTypes.depth,
#endif
            WmoWeatherCode.Haze => ShaderTypes.fog,
            WmoWeatherCode.Dust => ShaderTypes.fog,
            WmoWeatherCode.Mist => ShaderTypes.fog,
            WmoWeatherCode.Fog => ShaderTypes.fog,
            WmoWeatherCode.DepositingRimeFog => ShaderTypes.fog,
            WmoWeatherCode.LightDrizzle => ShaderTypes.rain,
            WmoWeatherCode.ModerateDrizzle => ShaderTypes.rain,
            WmoWeatherCode.DenseDrizzle => ShaderTypes.rain,
            WmoWeatherCode.LightFreezingDrizzle => ShaderTypes.rain,
            WmoWeatherCode.DenseFreezingDrizzle => ShaderTypes.rain,
            WmoWeatherCode.SlightRain => ShaderTypes.rain,
            WmoWeatherCode.ModerateRain => ShaderTypes.rain,
            WmoWeatherCode.HeavyRain => ShaderTypes.rain,
            WmoWeatherCode.LightFreezingRain => ShaderTypes.rain,
            WmoWeatherCode.HeavyFreezingRain => ShaderTypes.rain,
            WmoWeatherCode.SlightSnowFall => ShaderTypes.snow,
            WmoWeatherCode.ModerateSnowFall => ShaderTypes.snow,
            WmoWeatherCode.HeavySnowFall => ShaderTypes.snow,
            WmoWeatherCode.SnowGrains => ShaderTypes.snow,
            WmoWeatherCode.SlightRainShowers => ShaderTypes.rain,
            WmoWeatherCode.ModerateRainShowers => ShaderTypes.rain,
            WmoWeatherCode.ViolentRainShowers => ShaderTypes.rain,
            WmoWeatherCode.SlightSnowShowers => ShaderTypes.snow,
            WmoWeatherCode.HeavySnowShowers => ShaderTypes.snow,
            WmoWeatherCode.Thunderstorm => ShaderTypes.rain,
            WmoWeatherCode.ThunderstormLightHail => ShaderTypes.snow,
            WmoWeatherCode.ThunderstormHeavyHail => ShaderTypes.snow,
            _ => throw new NotImplementedException(),
        };
    }

    public static ShaderModel GetWeather(this WmoWeatherCode weatherCode)
    {
        return weatherCode switch
        {
            WmoWeatherCode.ClearSky => new DepthModel(),
            WmoWeatherCode.MainlyClear => new DepthModel(),
#if WINDOWS_UWP
            WmoWeatherCode.PartlyCloudy => new CloudsModel()
            {
                Scale = 0.45f,
            },
            WmoWeatherCode.Overcast => new CloudsModel(),
#else
            WmoWeatherCode.PartlyCloudy => new DepthModel(),
            WmoWeatherCode.Overcast => new DepthModel(),
#endif
            WmoWeatherCode.Haze => new WindModel()
            {
                Color1 = new Vector3(0.8f, .8f, .76f)
            },
            WmoWeatherCode.Dust => new WindModel()
            {
               Color1 = new Vector3(0.7f, .6f, .43f),
            },
            WmoWeatherCode.Mist => new WindModel()
            {
                Color1 = new Vector3(0.8f, .85f, .85f),
                Amplitude = 0.4f,
                Speed = 4.0f
            },
            WmoWeatherCode.Fog => new WindModel(),
            WmoWeatherCode.DepositingRimeFog => new WindModel()
            {
                // Snow RGB(255, 250, 250)
                Color1 = new Vector3(1f, .98f, .98f)
            },
            WmoWeatherCode.LightDrizzle => new RainModel()
            {
                PostProcessing = 0.25f,
                Intensity = 0.25f,
                Normal = 0.3f,
                Zoom = 1f,
                Speed = 0.05f
            },
            WmoWeatherCode.ModerateDrizzle => new RainModel()
            {
                PostProcessing = 0.35f,
                Intensity = 0.35f,
                Normal = 0.4f,
                Zoom = 1f,
                Speed = 0.1f
            },
            WmoWeatherCode.DenseDrizzle => new RainModel()
            {
                PostProcessing = 0.5f,
                Intensity = 0.4f,
                Normal = 0.5f,
                Zoom = 1f,
                Speed = 0.21f
            },
            WmoWeatherCode.LightFreezingDrizzle => new RainModel()
            {
                // Freezes on contact
                // ref: https://en.wikipedia.org/wiki/Freezing_rain
                IsFreezing = true,
                PostProcessing = 0.25f,
                Intensity = 0.25f,
                Normal = 0.3f,
                Zoom = 1f,
                Speed = 0.05f
            },
            WmoWeatherCode.DenseFreezingDrizzle => new RainModel()
            {
                IsFreezing = true,
                PostProcessing = 0.5f,
                Intensity = 0.4f,
                Normal = 0.5f,
                Zoom = 1f,
                Speed = 0.21f
            },
            WmoWeatherCode.SlightRain => new RainModel()
            {
                PostProcessing = 0.25f,
                Intensity = 0.25f,
                Normal = 0.3f,
                Zoom = 2f,
                Speed = 0.21f
            },
            WmoWeatherCode.ModerateRain => new RainModel()
            {
                PostProcessing = 0.5f,
                Intensity = 0.4f,
                Normal = 0.4f,
                Zoom = 2.61f,
                Speed = 0.25f
            },
            WmoWeatherCode.HeavyRain => new RainModel()
            {
                PostProcessing = 0.75f,
                Intensity = 0.5f,
                Normal = 0.5f,
                Zoom = 3f,
                Speed = 0.3f
            },
            WmoWeatherCode.LightFreezingRain => new RainModel()
            {
                IsFreezing = true,
                PostProcessing = 0.25f,
                Intensity = 0.25f,
                Normal = 0.5f,
                Zoom = 2f,
                Speed = 0.21f
            },
            WmoWeatherCode.HeavyFreezingRain => new RainModel()
            {
                IsFreezing = true,
                PostProcessing = 0.75f,
                Intensity = 0.5f,
                Normal = 0.5f,
                Zoom = 3f,
                Speed = 0.3f
            },
            WmoWeatherCode.SlightSnowFall => new SnowModel()
            {
                PostProcessing = 0.35f,
                Layers = 12,
                Depth = 0.25f,
                Speed = 0.3f
            },
            WmoWeatherCode.ModerateSnowFall => new SnowModel()
            {
                PostProcessing = 0.5f,
                Layers = 25,
                Depth = 0.5f,
                Speed = 0.6f
            },
            WmoWeatherCode.HeavySnowFall => new SnowModel()
            {
                PostProcessing = 0.75f,
                Layers = 50,
                Depth = 1f,
                Speed = 0.9f
            },
            WmoWeatherCode.SnowGrains => new SnowModel()
            {
                //ref: https://en.wikipedia.org/wiki/Snow_grains
                Layers = 12,
                Depth = 0.25f,
                Speed = 1.25f,
                PostProcessing = 0.5f,
            },
            WmoWeatherCode.SlightRainShowers => new RainModel()
            {
                PostProcessing = 0.5f,
                Intensity = 0.25f,
                Normal = 0.5f,
                Zoom = 2f,
                Speed = 0.3f,
            },
            WmoWeatherCode.ModerateRainShowers => new RainModel()
            {
                PostProcessing = 0.6f,
                Intensity = 0.4f,
                Normal = 0.5f,
                Zoom = 2.61f,
                Speed = 0.4f
            },
            WmoWeatherCode.ViolentRainShowers => new RainModel()
            {
                PostProcessing = 0.75f,
                Intensity = 0.6f,
                Normal = 0.5f,
                Zoom = 3f,
                Speed = 0.5f
            },
            WmoWeatherCode.SlightSnowShowers => new SnowModel()
            {
                Layers = 12,
                Depth = 0.25f,
                Speed = 0.3f,
                PostProcessing = 0.35f,
            },
            WmoWeatherCode.HeavySnowShowers => new SnowModel()
            {
                Layers = 25,
                Depth = 0.5f,
                Speed = 0.6f,
                PostProcessing = 0.75f,
            },
            WmoWeatherCode.Thunderstorm => new RainModel() 
            {
                Intensity = 0.4f,
                Normal = 0.5f,
                Zoom = 2.61f,
                Speed = 0.25f,
                IsLightning = true 
            },
            WmoWeatherCode.ThunderstormLightHail => new SnowModel()
            {
                Layers = 12,
                Depth = 0.25f,
                Speed = 2f,
                IsLightning = true,
                PostProcessing = 0.5f,
            },
            WmoWeatherCode.ThunderstormHeavyHail => new SnowModel()
            {
                Layers = 35,
                Depth = 1f,
                Speed = 2.5f,
                IsLightning = true,
                PostProcessing = 0.75f,
            },
            _ => throw new NotImplementedException(),
        };
    }
}
