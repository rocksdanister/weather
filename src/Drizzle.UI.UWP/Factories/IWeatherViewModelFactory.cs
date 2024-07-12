using Drizzle.Models.Enums;
using Drizzle.Models.Weather;
using Drizzle.UI.UWP.ViewModels;

namespace Drizzle.UI.UWP.Factories
{
    public interface IWeatherViewModelFactory
    {
        WeatherViewModel CreateWeatherViewModel(ForecastWeather weatherForecast, ForecastAirQuality airQualityForecast, int sortOrder, WeatherUnitSettings units, GraphType graphType);
        void UpdateGraphModels(WeatherViewModel model, GraphType graphType);
    }
}