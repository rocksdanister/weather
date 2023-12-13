using Drizzle.Common;
using Drizzle.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.UI.UWP.Factories
{
    public interface IWeatherClientFactory
    {
        IWeatherClient GetInstance(WeatherProviders provider);
    }
}
