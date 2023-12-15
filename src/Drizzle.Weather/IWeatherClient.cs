using Drizzle.Models.Weather;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.Weather;

public interface IWeatherClient
{
    string ApiKey { get; set; }

    Task<ForecastWeather> QueryForecastAsync(float latitude, float longitude);

    //Task<ForecastWeather> QueryForecastAsync(string place);

    Task<ForecastAirQuality> QueryAirQualityAsync(float latitude, float longitude);

    //Task<ForecastAirQuality> QueryAirQualityAsync(string place);

    Task<IReadOnlyList<Location>> GetLocationDataAsync(string place);
}