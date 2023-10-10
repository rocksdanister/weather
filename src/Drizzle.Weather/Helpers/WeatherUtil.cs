using Drizzle.Models.Weather;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TimeZoneConverter;

namespace Drizzle.Weather.Helpers;

public static class WeatherUtil
{
    public static ForecastWeather ToImperial(this ForecastWeather forecast)
    {
        if (forecast.Units.Unit != WeatherUnits.imperial)
        {
            var isMiles = forecast.Units.Unit == WeatherUnits.hybrid;
            for (int i = 0; i < 7; i++)
            {
                var weather = forecast.Daily[i];
                weather.TemperatureMin = CelsiusToFahrenheit(weather.TemperatureMin);
                weather.TemperatureMax = CelsiusToFahrenheit(weather.TemperatureMax);
                weather.ApparentTemperatureMin = CelsiusToFahrenheit(weather.ApparentTemperatureMin);
                weather.ApparentTemperatureMax = CelsiusToFahrenheit(weather.ApparentTemperatureMax);
                weather.Temperature = CelsiusToFahrenheit(weather.Temperature);
                weather.ApparentTemperature = CelsiusToFahrenheit(weather.ApparentTemperature);
                weather.DewPoint = CelsiusToFahrenheit(weather.DewPoint);
                weather.HourlyTemperature = weather.HourlyTemperature.Select(x => CelsiusToFahrenheit(x)).ToArray();

                if (!isMiles)
                {
                    weather.WindSpeed = KmToMi(weather.WindSpeed);
                    weather.GustSpeed = KmToMi(weather.GustSpeed);
                    weather.Visibility = KmToMi(weather.Visibility);
                    weather.HourlyVisibility = weather.HourlyVisibility.Select(x => KmToMi(x)).ToArray();
                    weather.HourlyWindSpeed = weather.HourlyWindSpeed.Select(x => KmToMi(x)).ToArray();
                }
            }
            forecast.Units = new ForecastWeatherUnits(WeatherUnits.imperial);
        }
        return forecast;
    }

    public static ForecastWeather ToMetric(this ForecastWeather forecast)
    {
        if (forecast.Units.Unit != WeatherUnits.metric)
        {
            var isCelsius = forecast.Units.Unit == WeatherUnits.hybrid;
            for (int i = 0; i < 7; i++)
            {
                var weather = forecast.Daily[i];
                weather.WindSpeed = MiToKm(weather.WindSpeed);
                weather.GustSpeed = MiToKm(weather.GustSpeed);
                weather.Visibility = MiToKm(weather.Visibility);
                weather.HourlyVisibility = weather.HourlyVisibility.Select(x => MiToKm(x)).ToArray();
                weather.HourlyWindSpeed = weather.HourlyWindSpeed.Select(x => MiToKm(x)).ToArray();

                if (!isCelsius)
                {
                    weather.TemperatureMin = FahrenheitToCelsius(weather.TemperatureMin);
                    weather.TemperatureMax = FahrenheitToCelsius(weather.TemperatureMax);
                    weather.ApparentTemperatureMin = FahrenheitToCelsius(weather.ApparentTemperatureMin);
                    weather.ApparentTemperatureMax = FahrenheitToCelsius(weather.ApparentTemperatureMax);
                    weather.Temperature = FahrenheitToCelsius(weather.Temperature);
                    weather.ApparentTemperature = FahrenheitToCelsius(weather.ApparentTemperature);
                    weather.DewPoint = FahrenheitToCelsius(weather.DewPoint);
                    weather.HourlyTemperature = weather.HourlyTemperature.Select(x => FahrenheitToCelsius(x)).ToArray();
                }
            }
            forecast.Units = new ForecastWeatherUnits(WeatherUnits.metric);
        }
        return forecast;
    }

    public static ForecastWeather ToHybrid(this ForecastWeather forecast)
    {
        if (forecast.Units.Unit != WeatherUnits.hybrid)
        {
            var isMiles = forecast.Units.Unit == WeatherUnits.imperial;
            var isCelsius = forecast.Units.Unit == WeatherUnits.metric;
            for (int i = 0; i < 7; i++)
            {
                var weather = forecast.Daily[i];
                if (!isCelsius)
                {
                    weather.TemperatureMin = FahrenheitToCelsius(weather.TemperatureMin);
                    weather.TemperatureMax = FahrenheitToCelsius(weather.TemperatureMax);
                    weather.ApparentTemperatureMin = FahrenheitToCelsius(weather.ApparentTemperatureMin);
                    weather.ApparentTemperatureMax = FahrenheitToCelsius(weather.ApparentTemperatureMax);
                    weather.Temperature = FahrenheitToCelsius(weather.Temperature);
                    weather.ApparentTemperature = FahrenheitToCelsius(weather.ApparentTemperature);
                    weather.DewPoint = FahrenheitToCelsius(weather.DewPoint);
                    weather.HourlyTemperature = weather.HourlyTemperature.Select(x => FahrenheitToCelsius(x)).ToArray();
                }

                if (!isMiles)
                {
                    weather.WindSpeed = KmToMi(weather.WindSpeed);
                    weather.GustSpeed = KmToMi(weather.GustSpeed);
                    weather.Visibility = KmToMi(weather.Visibility);
                    weather.HourlyVisibility = weather.HourlyVisibility.Select(x => KmToMi(x)).ToArray();
                    weather.HourlyWindSpeed = weather.HourlyWindSpeed.Select(x => KmToMi(x)).ToArray();
                }
            }
            forecast.Units = new ForecastWeatherUnits(WeatherUnits.hybrid);
        }
        return forecast;
    }

    public static string GetDayName(this DayOfWeek value, bool isAbbreviation = false)
    {
        return isAbbreviation ? 
            DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName(value) : DateTimeFormatInfo.CurrentInfo.GetDayName(value);
    }

    public static DateTime? GetLocalTime(DateTime time, string timezone)
    {
        try
        {
            var timeZoneInfo = TZConvert.GetTimeZoneInfo(timezone);
            return TimeZoneInfo.ConvertTime(time, timeZoneInfo);
        }
        catch
        {
            return null;
        }
    }

    public static float CelsiusToFahrenheit(float celsius) => celsius * 9f / 5f + 32f;

    public static float FahrenheitToCelsius(float fahrenheit) => (fahrenheit - 32f) * 5f / 9f;

    public static float KmToMi(float speed) => speed / 1.6093440006147f;

    public static float MiToKm(float distance) => distance * 1.6093440006147f;

    public static float KmToFt(float distace) => distace * 3280.839895f;

    public static float FtToKm(float feet) => feet / 3280.839895f;
}
