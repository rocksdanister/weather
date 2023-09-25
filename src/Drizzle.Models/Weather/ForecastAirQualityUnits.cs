using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.Weather
{
    public class ForecastAirQualityUnits
    {
        public ForecastAirQualityUnits(WeatherUnits unit)
        {
            this.Unit = unit;
        }

        public WeatherUnits Unit { get; private set; }
    }
}
