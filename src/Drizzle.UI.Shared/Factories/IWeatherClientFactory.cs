using Drizzle.Models.Weather;
using Drizzle.Weather;

namespace Drizzle.UI.Shared.Factories;

public interface IWeatherClientFactory
{
    IWeatherClient GetInstance(WeatherProviders provider);
}
