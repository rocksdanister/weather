using Drizzle.Models.Enums;
using Drizzle.Models.UserControls;
using Drizzle.Models.Weather;
using Drizzle.UI.UWP.ViewModels;
using System.Collections.Generic;

namespace Drizzle.UI.UWP.Factories
{
    public interface IWeatherViewModelFactory
    {
        WeatherViewModel CreateWeatherViewModel(ForecastWeather weatherForecast, ForecastAirQuality airQualityForecast, int sortOrder, WeatherUnitSettings units, GraphType graphType);
        List<GraphModel> CreateGraphModels(WeatherViewModel weather, GraphType graphType);
    }
}