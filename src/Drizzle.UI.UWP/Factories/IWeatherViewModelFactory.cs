using Drizzle.Models.Weather;
using Drizzle.UI.UWP.ViewModels;

namespace Drizzle.UI.UWP.Factories
{
    public interface IWeatherViewModelFactory
    {
        WeatherViewModel CreateWeatherViewModel(ForecastWeather weatherForecast, ForecastAirQuality airQualityForecast, int sortOrder, WeatherUnits units = WeatherUnits.metric);
    }
}