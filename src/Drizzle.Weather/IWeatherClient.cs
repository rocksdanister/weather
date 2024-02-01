using Drizzle.Models.Weather;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.Weather;

public interface IWeatherClient
{
    string ApiKey { get; set; }
    bool IsApiKeyRequired { get; }
    bool IsReverseGeocodingSupported { get; }

    Task<ForecastWeather> QueryForecastAsync(float latitude, float longitude);
    Task<ForecastAirQuality> QueryAirQualityAsync(float latitude, float longitude);
    Task<IReadOnlyList<Location>> GetLocationDataAsync(string place);
    Task<IReadOnlyList<Location>> GetLocationDataAsync(float latitude, float longitude);
}