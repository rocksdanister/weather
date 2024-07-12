using Drizzle.Models.Weather;
using Drizzle.Weather;

namespace Drizzle.UI.UWP.Factories
{
    public interface IWeatherClientFactory
    {
        IWeatherClient GetInstance(WeatherProviders provider);
    }
}
