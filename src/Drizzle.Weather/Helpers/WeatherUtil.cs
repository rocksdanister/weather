using Drizzle.Common;
using Drizzle.Models.Weather;
using Drizzle.Models.Weather.OpenMeteo;
using GeoTimeZone;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TimeZoneConverter;

namespace Drizzle.Weather.Helpers;

public static class WeatherUtil
{
    public static void ToImperialUnit(this ForecastWeather forecast) =>
        ConvertUnit(forecast, new WeatherUnitSettings(WeatherUnits.imperial));

    public static void ToMetricUnit(this ForecastWeather forecast) =>
        ConvertUnit(forecast, new WeatherUnitSettings(WeatherUnits.metric));

    public static void ToHybridUnit(this ForecastWeather forecast) =>
        ConvertUnit(forecast, new WeatherUnitSettings(WeatherUnits.hybrid));

    public static void ToCustomUnit(this ForecastWeather forecast, WeatherUnitSettings toUnit) =>
        ConvertUnit(forecast, toUnit);

    public static void ToCustomUnit(this ForecastWeather forecast,
                                               TemperatureUnits temperatureUnit,
                                               WindSpeedUnits windSpeedUnit,
                                               VisibilityUnits visibilityUnit,
                                               PressureUnits pressureUnit,
                                               PrecipitationUnits precipitationUnit) =>
        ConvertUnit(forecast, new WeatherUnitSettings(temperatureUnit, windSpeedUnit, visibilityUnit, pressureUnit, precipitationUnit));

    public static string GetUnitString(this WindSpeedUnits unit)
    {
        return unit switch
        {
            WindSpeedUnits.kmh => "km/h",
            WindSpeedUnits.mph => "mph",
            WindSpeedUnits.ms => "m/s",
            _ => throw new NotImplementedException(),
        };
    }

    public static string GetUnitString(this TemperatureUnits unit)
    {
        return unit switch
        {
            TemperatureUnits.fahrenheit => "°F",
            TemperatureUnits.degree => "°C",
            _ => throw new NotImplementedException(),
        };
    }

    public static string GetUnitString(this VisibilityUnits unit)
    {
        return unit switch
        {
            VisibilityUnits.km => "km",
            VisibilityUnits.mi => "mi",
            _ => throw new NotImplementedException(),
        };
    }

    public static string GetUnitString(this PressureUnits unit)
    {
        return unit switch
        {
            PressureUnits.hPa_mb => "hPa/mb",
            _ => throw new NotImplementedException(),
        };
    }

    public static string GetUnitString(this PrecipitationUnits unit)
    {
        return unit switch
        {
            PrecipitationUnits.mm => "mm",
            PrecipitationUnits.inch => "inch",
            _ => throw new NotImplementedException(),
        };
    }

    public static WeatherSeverityLevel GetSeverity(WmoWeatherCode weatherCode)
    {
        switch (weatherCode)
        {
            case WmoWeatherCode.ClearSky:
            case WmoWeatherCode.MainlyClear:
            case WmoWeatherCode.PartlyCloudy:
            case WmoWeatherCode.Overcast:
            case WmoWeatherCode.Haze:
            case WmoWeatherCode.Dust:
                return WeatherSeverityLevel.NotSevere;
            case WmoWeatherCode.Mist:
            case WmoWeatherCode.Fog:
            case WmoWeatherCode.DepositingRimeFog:
            case WmoWeatherCode.LightDrizzle:
            case WmoWeatherCode.ModerateDrizzle:
            case WmoWeatherCode.SlightRain:
            case WmoWeatherCode.SlightSnowFall:
            case WmoWeatherCode.SnowGrains:
            case WmoWeatherCode.SlightRainShowers:
            case WmoWeatherCode.SlightSnowShowers:
                return WeatherSeverityLevel.Mild;
            case WmoWeatherCode.DenseDrizzle:
            case WmoWeatherCode.LightFreezingDrizzle:
            case WmoWeatherCode.DenseFreezingDrizzle:
            case WmoWeatherCode.ModerateRain:
            case WmoWeatherCode.LightFreezingRain:
            case WmoWeatherCode.ModerateSnowFall:
            case WmoWeatherCode.ModerateRainShowers:
                return WeatherSeverityLevel.Moderate;
            case WmoWeatherCode.HeavyRain:
            case WmoWeatherCode.HeavyFreezingRain:
            case WmoWeatherCode.HeavySnowFall:
            case WmoWeatherCode.ViolentRainShowers:
            case WmoWeatherCode.HeavySnowShowers:
            case WmoWeatherCode.Thunderstorm:
            case WmoWeatherCode.ThunderstormLightHail:
            case WmoWeatherCode.ThunderstormHeavyHail:
                return WeatherSeverityLevel.Severe;
            default:
                throw new NotImplementedException();
        }
    }

    private static void ConvertUnit(ForecastWeather forecast, WeatherUnitSettings toUnit)
    {
        if (forecast.Units.Unit != WeatherUnits.custom && forecast.Units.Unit == toUnit.Unit)
            return;

        for (int i = 0; i < forecast.Daily.Count; i++)
        {
            var weather = forecast.Daily[i];
            switch (forecast.Units.TemperatureUnit)
            {
                case TemperatureUnits.degree:
                    {
                        switch (toUnit.TemperatureUnit)
                        {
                            case TemperatureUnits.degree:
                                // Requested unit same as current.
                                break;
                            case TemperatureUnits.fahrenheit:
                                {
                                    weather.TemperatureMin = CelsiusToFahrenheit(weather.TemperatureMin);
                                    weather.TemperatureMax = CelsiusToFahrenheit(weather.TemperatureMax);
                                    weather.ApparentTemperatureMin = CelsiusToFahrenheit(weather.ApparentTemperatureMin);
                                    weather.ApparentTemperatureMax = CelsiusToFahrenheit(weather.ApparentTemperatureMax);
                                    weather.Temperature = CelsiusToFahrenheit(weather.Temperature);
                                    weather.ApparentTemperature = CelsiusToFahrenheit(weather.ApparentTemperature);
                                    weather.DewPoint = CelsiusToFahrenheit(weather.DewPoint);
                                    weather.HourlyTemperature = weather.HourlyTemperature?.Select(x => (float)CelsiusToFahrenheit(x)).ToArray();
                                    weather.HourlyApparentTemperature = weather.HourlyApparentTemperature?.Select(x => (float)CelsiusToFahrenheit(x)).ToArray();
                                }
                                break;
                        }
                    }
                    break;
                case TemperatureUnits.fahrenheit:
                    switch (toUnit.TemperatureUnit)
                    {
                        case TemperatureUnits.degree:
                            {
                                weather.TemperatureMin = FahrenheitToCelsius(weather.TemperatureMin);
                                weather.TemperatureMax = FahrenheitToCelsius(weather.TemperatureMax);
                                weather.ApparentTemperatureMin = FahrenheitToCelsius(weather.ApparentTemperatureMin);
                                weather.ApparentTemperatureMax = FahrenheitToCelsius(weather.ApparentTemperatureMax);
                                weather.Temperature = FahrenheitToCelsius(weather.Temperature);
                                weather.ApparentTemperature = FahrenheitToCelsius(weather.ApparentTemperature);
                                weather.DewPoint = FahrenheitToCelsius(weather.DewPoint);
                                weather.HourlyTemperature = weather.HourlyTemperature?.Select(x => (float)FahrenheitToCelsius(x)).ToArray();
                                weather.HourlyApparentTemperature = weather.HourlyApparentTemperature?.Select(x => (float)FahrenheitToCelsius(x)).ToArray();
                            }
                            break;
                        case TemperatureUnits.fahrenheit:
                            break;
                    }
                    break;
            }

            switch (forecast.Units.VisibilityUnit)
            {
                case VisibilityUnits.km:
                    switch (toUnit.VisibilityUnit)
                    {
                        case VisibilityUnits.km:
                            break;
                        case VisibilityUnits.mi:
                            {
                                weather.Visibility = KmToMi(weather.Visibility);
                                weather.HourlyVisibility = weather.HourlyVisibility?.Select(x => (float)KmToMi(x)).ToArray();
                            }
                            break;
                    }
                    break;
                case VisibilityUnits.mi:
                    switch (toUnit.VisibilityUnit)
                    {
                        case VisibilityUnits.km:
                            {
                                weather.Visibility = MiToKm(weather.Visibility);
                                weather.HourlyVisibility = weather.HourlyVisibility?.Select(x => (float)MiToKm(x)).ToArray();
                            }
                            break;
                        case VisibilityUnits.mi:
                            break;
                    }
                    break;
            }

            switch (forecast.Units.WindSpeedUnit)
            {
                case WindSpeedUnits.kmh:
                    switch (toUnit.WindSpeedUnit)
                    {
                        case WindSpeedUnits.kmh:
                            break;
                        case WindSpeedUnits.mph:
                            {
                                weather.WindSpeed = KmToMi(weather.WindSpeed);
                                weather.GustSpeed = KmToMi(weather.GustSpeed);
                                weather.HourlyWindSpeed = weather.HourlyWindSpeed?.Select(x => (float)KmToMi(x)).ToArray();
                            }
                            break;
                        case WindSpeedUnits.ms:
                            {
                                weather.WindSpeed = KmhToMs(weather.WindSpeed);
                                weather.GustSpeed = KmhToMs(weather.GustSpeed);
                                weather.HourlyWindSpeed = weather.HourlyWindSpeed?.Select(x => (float)KmhToMs(x)).ToArray();
                            }
                            break;
                    }
                    break;
                case WindSpeedUnits.mph:
                    switch (toUnit.WindSpeedUnit)
                    {
                        case WindSpeedUnits.kmh:
                            {
                                weather.WindSpeed = MiToKm(weather.WindSpeed);
                                weather.GustSpeed = MiToKm(weather.GustSpeed);
                                weather.HourlyWindSpeed = weather.HourlyWindSpeed?.Select(x => (float)MiToKm(x)).ToArray();
                            }
                            break;
                        case WindSpeedUnits.mph:
                            break;
                        case WindSpeedUnits.ms:
                            {
                                weather.WindSpeed = MphToMs(weather.WindSpeed);
                                weather.GustSpeed = MphToMs(weather.GustSpeed);
                                weather.HourlyWindSpeed = weather.HourlyWindSpeed?.Select(x => (float)MphToMs(x)).ToArray();
                            }
                            break;
                    }
                    break;
                case WindSpeedUnits.ms:
                    switch (toUnit.WindSpeedUnit)
                    {
                        case WindSpeedUnits.kmh:
                            {
                                weather.WindSpeed = MsToKmh(weather.WindSpeed);
                                weather.GustSpeed = MsToKmh(weather.GustSpeed);
                                weather.HourlyWindSpeed = weather.HourlyWindSpeed?.Select(x => (float)MsToKmh(x)).ToArray();
                            }
                            break;
                        case WindSpeedUnits.mph:
                            {
                                weather.WindSpeed = MsToMph(weather.WindSpeed);
                                weather.GustSpeed = MsToMph(weather.GustSpeed);
                                weather.HourlyWindSpeed = weather.HourlyWindSpeed?.Select(x => (float)MsToMph(x)).ToArray();
                            }
                            break;
                        case WindSpeedUnits.ms:
                            break;
                    }
                    break;
            }

            switch (forecast.Units.PrecipitationUnit)
            {
                case PrecipitationUnits.mm:
                    switch (toUnit.PrecipitationUnit)
                    {
                        case PrecipitationUnits.mm:
                            break;
                        case PrecipitationUnits.inch:
                            {
                                weather.Precipitation = MmtoInch(weather.Precipitation);
                                weather.HourlyPrecipitation = weather.HourlyPrecipitation?.Select(x => (float)MmtoInch(x)).ToArray();
                            }
                            break;
                    }
                    break;
                case PrecipitationUnits.inch:
                    switch (toUnit.PrecipitationUnit)
                    {
                        case PrecipitationUnits.mm:
                            {
                                weather.Precipitation = InchToMm(weather.Precipitation);
                                weather.HourlyPrecipitation = weather.HourlyPrecipitation?.Select(x => (float)InchToMm(x)).ToArray();
                            }
                            break;
                        case PrecipitationUnits.inch:
                            break;
                    }
                    break;
            }

            //switch (forecast.Units.PressureUnit)
            //{
            //    case PressureUnits.hPa_mb:
            //        break;
            //}
        }
        forecast.Units = toUnit;
    }

    // Lifted operators
    // Ref: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types

    public static float? CelsiusToFahrenheit(float? celsius) => celsius * 9f / 5f + 32f;

    public static float? FahrenheitToCelsius(float? fahrenheit) => (fahrenheit - 32f) * 5f / 9f;

    public static float? KmToMi(float? speed) => speed / 1.6093440006147f;

    public static float? MiToKm(float? distance) => distance * 1.6093440006147f;

    public static float? KmToFt(float? distace) => distace * 3280.839895f;

    public static float? FtToKm(float? feet) => feet / 3280.839895f;

    public static float? KmhToMs(float? speed) => speed / 3.6f;

    public static float? MsToKmh(float? speed) => speed * 3.6f;

    public static float? MphToMs(float? speed) => speed / 2.237f;

    public static float? MsToMph(float? speed) => speed * 2.237f;

    public static float? MmtoInch(float? lenght) => lenght / 25.4f;

    public static float? InchToMm(float? lenght) => lenght * 25.4f;

    public enum WeatherSeverityLevel
    {
        NotSevere = 0,
        Mild = 1,
        Moderate = 2,
        Severe = 3
    }
}
