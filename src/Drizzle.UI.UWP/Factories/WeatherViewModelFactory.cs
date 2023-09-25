using Drizzle.Models.Weather;
using Drizzle.UI.UWP.ViewModels;
using System.Linq;
using Drizzle.Weather.Helpers;
using Drizzle.Models;

namespace Drizzle.UI.UWP.Factories
{
    public class WeatherViewModelFactory : IWeatherViewModelFactory
    {
        public WeatherViewModel CreateWeatherViewModel(
            ForecastWeather weatherForecast,
            ForecastAirQuality airQualityForecast,
            WeatherUnits units = WeatherUnits.metric)
        {
            var weatherVm = new WeatherViewModel();
            switch (units)
            {
                case WeatherUnits.metric:
                    weatherForecast.ToMetric();
                    break;
                case WeatherUnits.imperial:
                    weatherForecast.ToImperial();
                    break;
            }

            weatherVm.TimeZone = weatherForecast.TimeZone;
            weatherVm.FetchTime = weatherForecast.FetchTime;
            weatherVm.Location = new LocationModel(weatherForecast.Name, weatherForecast.Latitude, weatherForecast.Longitude);
            for (int i = 0; i < weatherForecast.Daily.Count(); i++)
            {
                var tmp = new WeatherModel()
                {
                    LocationName = weatherForecast.Name,
                    WeatherCode = weatherForecast.Daily[i].WeatherCode,
                    Temperature = weatherForecast.Daily[i].Temperature,
                    TemperatureUnit = weatherForecast.Units.TemperatureUnit,
                    FeelsLike = weatherForecast.Daily[i].ApparentTemperature,
                    WindSpeed = weatherForecast.Daily[i].WindSpeed,
                    GustSpeed = weatherForecast.Daily[i].GustSpeed,
                    WindSpeedUnit = weatherForecast.Units.WindSpeedUnit,
                    WindDirection = weatherForecast.Daily[i].WindDirection,
                    Humidity = weatherForecast.Daily[i].Humidity,
                    Visibility = weatherForecast.Daily[i].Visibility,
                    VisibilityUnit = weatherForecast.Units.VisibilityUnit,
                    Pressure = weatherForecast.Daily[i].Pressure,
                    PressureUnit = weatherForecast.Units.PressureUnit,
                    DewPoint = weatherForecast.Daily[i].DewPoint,
                    HourlyTemp = weatherForecast.Daily[i].HourlyTemperature,
                    HourlyVisibility = weatherForecast.Daily[i].HourlyVisibility,
                    HourlyHumidity = weatherForecast.Daily[i].HourlyHumidity,
                    HourlyPressure = weatherForecast.Daily[i].HourlyPressure,
                    HourlyWindSpeed = weatherForecast.Daily[i].HourlyWindSpeed,
                    Date = weatherForecast.Daily[i].Date,
                    Sunrise = weatherForecast.Daily[i].Sunrise,
                    Sunset = weatherForecast.Daily[i].Sunset
                };
                weatherVm.Daily.Add(tmp);
            }
            for (int i = 0; i < airQualityForecast.Daily.Count(); i++)
            {
                weatherVm.Daily[i].AirQualityIndex = airQualityForecast.Daily[i].AQI;
                weatherVm.Daily[i].UltravioletIndex = (int)airQualityForecast.Daily[i].UV;
                weatherVm.Daily[i].HourlyAirQualityIndex = airQualityForecast.Daily[i].HourlyAQI;
                weatherVm.Daily[i].HourlyUltravioletIndex = airQualityForecast.Daily[i].HourlyUV;
            }
            weatherVm.Today = weatherVm.Daily[0];

            return weatherVm;
        }
    }
}
