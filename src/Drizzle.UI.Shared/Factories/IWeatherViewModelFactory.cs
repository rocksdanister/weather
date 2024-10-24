using Drizzle.Models.Enums;
using Drizzle.Models.UserControls;
using Drizzle.Models.Weather;
using Drizzle.UI.Shared.ViewModels;
using System.Collections.Generic;

namespace Drizzle.UI.Shared.Factories;

public interface IWeatherViewModelFactory
{
    WeatherViewModel CreateWeatherViewModel(ForecastWeather weatherForecast, ForecastAirQuality airQualityForecast, int sortOrder, WeatherUnitSettings units, GraphType graphType);
    List<GraphModel> CreateGraphModels(WeatherViewModel weather, GraphType graphType);
}