using Drizzle.Models.Weather;
using Drizzle.Weather;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drizzle.UI.Shared.Factories;

public class WeatherClientFactory : IWeatherClientFactory
{
    private readonly IEnumerable<IWeatherClient> weatherClients;

    public WeatherClientFactory(IEnumerable<IWeatherClient> weatherClients)
    {
        this.weatherClients = weatherClients;
    }

    public IWeatherClient GetInstance(WeatherProviders provider)
    {
        return provider switch
        {
            WeatherProviders.OpenMeteo => GetService(typeof(OpenMeteoWeatherClient)),
            WeatherProviders.OpenWeatherMap => GetService(typeof(OpenWeatherMapWeatherClient)),
            WeatherProviders.Qweather => GetService(typeof(QweatherWeatherClient)),            
            _ => throw new NotImplementedException(),
        };
    }

    private IWeatherClient GetService(Type type) => weatherClients.FirstOrDefault(x => x.GetType() == type);
}
