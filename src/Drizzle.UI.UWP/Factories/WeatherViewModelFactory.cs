using Drizzle.Models.Weather;
using Drizzle.UI.UWP.ViewModels;
using System.Linq;
using Drizzle.Weather.Helpers;
using Drizzle.Models;
using System;

namespace Drizzle.UI.UWP.Factories
{
    public class WeatherViewModelFactory : IWeatherViewModelFactory
    {
        public WeatherViewModel CreateWeatherViewModel(
            ForecastWeather weatherForecast,
            ForecastAirQuality airQualityForecast,
            int sortOrder,
            WeatherUnitSettings units)
        {
            var weatherVm = new WeatherViewModel();
            weatherVm.SortOrder = sortOrder;
            weatherForecast.ToCustomUnit(units);

            weatherVm.TimeZone = weatherForecast.TimeZone;
            weatherVm.FetchTime = weatherForecast.FetchTime;
            weatherVm.Location = new LocationModel(weatherForecast.Name, weatherForecast.Latitude, weatherForecast.Longitude);
            weatherVm.MaxTemp = weatherForecast.Daily[0].TemperatureMax;
            weatherVm.MinTemp = weatherForecast.Daily[0].TemperatureMin;
            weatherVm.ForecastInterval = weatherForecast.ForecastInterval;
            weatherVm.ForecastAQInterval = airQualityForecast.ForecastInterval;
            var isDayTime = TimeUtil.IsDaytime(weatherForecast.TimeZone);
            for (int i = 0; i < weatherForecast.Daily.Count(); i++)
            {
                var tmp = new WeatherModel()
                {
                    LocationName = weatherForecast.Name,
                    WeatherCode = weatherForecast.Daily[i].WeatherCode,
                    Temperature = weatherForecast.Daily[i].Temperature,
                    TemperatureMin = weatherForecast.Daily[i].TemperatureMin,
                    TemperatureMax = weatherForecast.Daily[i].TemperatureMax,
                    TemperatureUnit = weatherForecast.Units.TemperatureUnit.GetUnitString(),
                    FeelsLike = weatherForecast.Daily[i].ApparentTemperature,
                    WindSpeed = weatherForecast.Daily[i].WindSpeed,
                    GustSpeed = weatherForecast.Daily[i].GustSpeed,
                    WindSpeedUnit = weatherForecast.Units.WindSpeedUnit.GetUnitString(),
                    WindDirection = weatherForecast.Daily[i].WindDirection,
                    Humidity = weatherForecast.Daily[i].Humidity,
                    Visibility = weatherForecast.Daily[i].Visibility,
                    VisibilityUnit = weatherForecast.Units.VisibilityUnit.GetUnitString(),
                    Pressure = weatherForecast.Daily[i].Pressure,
                    PressureUnit = weatherForecast.Units.PressureUnit.GetUnitString(),
                    DewPoint = weatherForecast.Daily[i].DewPoint,
                    HourlyWeatherCode = weatherForecast.Daily[i].HourlyWeatherCode,
                    HourlyTemp = weatherForecast.Daily[i].HourlyTemperature,
                    HourlyVisibility = weatherForecast.Daily[i].HourlyVisibility,
                    HourlyHumidity = weatherForecast.Daily[i].HourlyHumidity,
                    HourlyPressure = weatherForecast.Daily[i].HourlyPressure,
                    HourlyWindSpeed = weatherForecast.Daily[i].HourlyWindSpeed,
                    ForecastStartTime = weatherForecast.Daily[i].StartTime,
                    Sunrise = weatherForecast.Daily[i].Sunrise,
                    Sunset = weatherForecast.Daily[i].Sunset,
                    IsDaytime = isDayTime,
                };
                weatherVm.MaxTemp = tmp.TemperatureMax > weatherVm.MaxTemp ? tmp.TemperatureMax : weatherVm.MaxTemp;
                weatherVm.MinTemp = tmp.TemperatureMin < weatherVm.MinTemp ? tmp.TemperatureMin : weatherVm.MinTemp;
                weatherVm.Daily.Add(tmp);
            }
            for (int i = 0; i < airQualityForecast.Daily.Count(); i++)
            {
                weatherVm.Daily[i].AirQualityIndex = airQualityForecast.Daily[i].AQI ?? null;
                weatherVm.Daily[i].UltravioletIndex = (int?)airQualityForecast.Daily[i].UV ?? null;
                weatherVm.Daily[i].HourlyAirQualityIndex = airQualityForecast.Daily[i].HourlyAQI;
                weatherVm.Daily[i].HourlyUltravioletIndex = airQualityForecast.Daily[i].HourlyUV;
                weatherVm.Daily[i].ForecastAQStartTime = airQualityForecast.Daily[i].StartTime;
            }
            weatherVm.Today = weatherVm.Daily[0];
            
            return weatherVm;
        }
    }
}
