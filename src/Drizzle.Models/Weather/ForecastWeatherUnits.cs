using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.Weather
{
    public class ForecastWeatherUnits
    {
        public ForecastWeatherUnits(WeatherUnits unit) 
        {
            switch (unit)
            {
                case WeatherUnits.metric:
                    {
                        TemperatureUnitString = "°C";
                        WindSpeedUnitString = "kmh";
                        VisibilityUnitString = "km";
                        PressureUnitString = "hPa/mb";
                    }
                    break;
                case WeatherUnits.imperial:
                    {
                        TemperatureUnitString = "°F";
                        WindSpeedUnitString = "mph";
                        VisibilityUnitString = "mi";
                        PressureUnitString = "hPa/mb";
                    }
                    break;
                case WeatherUnits.hybrid:
                    {
                        TemperatureUnitString = "°C";
                        WindSpeedUnitString = "mph";
                        VisibilityUnitString = "mi";
                        PressureUnitString = "hPa/mb";
                    }
                    break;
            }
            this.Unit = unit;
        }

        public WeatherUnits Unit { get; private set; }

        public string TemperatureUnitString { get; private set; }

        public string WindSpeedUnitString { get; private set; }

        public string VisibilityUnitString { get; private set; }

        public string PressureUnitString { get; private set; }
    }
}
