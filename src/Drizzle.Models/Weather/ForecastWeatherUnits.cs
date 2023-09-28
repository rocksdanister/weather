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
                        TemperatureUnit = "°C";
                        WindSpeedUnit = "kmh";
                        VisibilityUnit = "km";
                        PressureUnit = "hPa/mb";
                    }
                    break;
                case WeatherUnits.imperial:
                    {
                        TemperatureUnit = "°F";
                        WindSpeedUnit = "mph";
                        VisibilityUnit = "mi";
                        PressureUnit = "hPa/mb";
                    }
                    break;
                case WeatherUnits.hybrid:
                    {
                        TemperatureUnit = "°C";
                        WindSpeedUnit = "mph";
                        VisibilityUnit = "mi";
                        PressureUnit = "hPa/mb";
                    }
                    break;
            }
            this.Unit = unit;
        }

        public WeatherUnits Unit { get; private set; }

        public string TemperatureUnit { get; private set; }

        public string WindSpeedUnit { get; private set; }

        public string VisibilityUnit { get; private set; }

        public string PressureUnit { get; private set; }
    }
}
