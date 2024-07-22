using Drizzle.Models;
using Drizzle.Models.Enums;
using Drizzle.Models.UserControls;
using Drizzle.Models.Weather;
using Drizzle.UI.UWP.ViewModels;
using Drizzle.Weather.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drizzle.UI.UWP.Factories
{
    public class WeatherViewModelFactory : IWeatherViewModelFactory
    {
        public WeatherViewModel CreateWeatherViewModel(
            ForecastWeather weatherForecast,
            ForecastAirQuality airQualityForecast,
            int sortOrder,
            WeatherUnitSettings units,
            GraphType graphType)
        {
            var weatherVm = new WeatherViewModel();
            weatherVm.SortOrder = sortOrder;
            weatherForecast.ToCustomUnit(units);

            weatherVm.TimeZone = weatherForecast.TimeZone;
            weatherVm.FetchTime = weatherForecast.FetchTime;
            weatherVm.Location = new LocationModel(weatherForecast.Name, weatherForecast.Latitude, weatherForecast.Longitude);
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
                    VisibilityUnit = weatherForecast.Units.VisibilityUnit,
                    Pressure = weatherForecast.Daily[i].Pressure,
                    PressureUnit = weatherForecast.Units.PressureUnit.GetUnitString(),
                    DewPoint = weatherForecast.Daily[i].DewPoint,
                    CloudCover = weatherForecast.Daily[i].CloudCover,
                    Precipitation = weatherForecast.Daily[i].Precipitation,
                    PrecipitationUnit = weatherForecast.Units.PrecipitationUnit,
                    HourlyWeatherCode = weatherForecast.Daily[i].HourlyWeatherCode,
                    HourlyTemp = weatherForecast.Daily[i].HourlyTemperature,
                    HourlyApparentTemperature = weatherForecast.Daily[i].HourlyApparentTemperature,
                    HourlyVisibility = weatherForecast.Daily[i].HourlyVisibility,
                    HourlyHumidity = weatherForecast.Daily[i].HourlyHumidity,
                    HourlyPressure = weatherForecast.Daily[i].HourlyPressure,
                    HourlyWindSpeed = weatherForecast.Daily[i].HourlyWindSpeed,
                    HourlyCloudCover = weatherForecast.Daily[i].HourlyCloudCover,
                    HourlyPrecipitation = weatherForecast.Daily[i].HourlyPrecipitation,
                    ForecastStartTime = weatherForecast.Daily[i].StartTime,
                    Sunrise = weatherForecast.Daily[i].Sunrise,
                    Sunset = weatherForecast.Daily[i].Sunset,
                    IsDaytime = isDayTime,
                };
                weatherVm.Daily.Add(tmp);
            }
            for (int i = 0; i < airQualityForecast.Daily.Count(); i++)
            {
                // If the weather data is less than the air quality.
                if (weatherVm.Daily.Count < i + 1)
                    break;

                weatherVm.Daily[i].AirQualityIndex = airQualityForecast.Daily[i].AQI ?? null;
                weatherVm.Daily[i].UltravioletIndex = (int?)airQualityForecast.Daily[i].UV ?? null;
                weatherVm.Daily[i].HourlyAirQualityIndex = airQualityForecast.Daily[i].HourlyAQI;
                weatherVm.Daily[i].HourlyUltravioletIndex = airQualityForecast.Daily[i].HourlyUV;
                weatherVm.Daily[i].ForecastAQStartTime = airQualityForecast.Daily[i].StartTime;
            }
            weatherVm.Today = weatherVm.Daily[0];

            // Update main graph
            var graphs = CreateGraphModels(weatherVm, graphType);
            for (int i = 0; i < weatherVm.Daily.Count; i++)
                weatherVm.Daily[i].DayGraph = graphs[i];

            return weatherVm;
        }

        public List<GraphModel> CreateGraphModels(WeatherViewModel weather, GraphType graphType)
        {
            switch (graphType)
            {
                case GraphType.temperature:
                    {
                        var result = new List<GraphModel>();
                        var allTemps = weather.Daily
                            .Where(x => x.HourlyTemp != null && x.HourlyTemp.Any())
                            .SelectMany(x => x.HourlyTemp);
                        float? maxTemp = allTemps.Any() ? allTemps.Max() : null;
                        float? minTemp = allTemps.Any() ? allTemps.Min() : null;
                        foreach (var item in weather.Daily)
                        {
                            var model = new GraphModel()
                            {
                                MaxValue = maxTemp,
                                MinValue = minTemp,
                                Interval = weather.ForecastInterval,
                                StartTime = item.ForecastStartTime,
                                ValueFormat = "{0:F0}°",
                                Value = item.HourlyTemp
                            };
                            result.Add(model);
                        }
                        return result;
                    }
                case GraphType.apparentTemperature:
                    {
                        var result = new List<GraphModel>();
                        var allTemps = weather.Daily
                            .Where(x => x.HourlyApparentTemperature != null && x.HourlyApparentTemperature.Any())
                            .SelectMany(x => x.HourlyApparentTemperature);
                        float? maxTemp = allTemps.Any() ? allTemps.Max() : null;
                        float? minTemp = allTemps.Any() ? allTemps.Min() : null;
                        foreach (var item in weather.Daily)
                        {
                            var model = new GraphModel()
                            {
                                MaxValue = maxTemp,
                                MinValue = minTemp,
                                Interval = weather.ForecastInterval,
                                StartTime = item.ForecastStartTime,
                                ValueFormat = "{0:F0}°",
                                Value = item.HourlyApparentTemperature
                            };
                            result.Add(model);
                        }
                        return result;
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
