using Drizzle.Common.Services;
using Drizzle.Models.Weather;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.Weather;

public class OpenWeatherMapWeatherClient : IWeatherClient
{
    public bool UseCache { get; set; } = false;

    private readonly HttpClient httpClient;
    private readonly string apiKey;

    public OpenWeatherMapWeatherClient(IHttpClientFactory httpClientFactory, string apiKey)
    {
        this.httpClient = httpClientFactory.CreateClient();
        this.apiKey = apiKey;
    }

    public Task<IReadOnlyList<Location>> GetLocationDataAsync(string place)
    {
        throw new NotImplementedException();
    }

    public Task<ForecastAirQuality> QueryAirQualityAsync(float latitude, float longitude)
    {
        throw new NotImplementedException();
    }

    public Task<ForecastWeather> QueryForecastAsync(float latitude, float longitude)
    {
        throw new NotImplementedException();
    }
}
