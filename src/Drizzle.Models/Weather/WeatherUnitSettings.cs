using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.Weather
{
    public class WeatherUnitSettings
    {
        public WeatherUnitSettings(WeatherUnits unit) 
        {
            switch (unit)
            {
                case WeatherUnits.metric:
                    {
                        TemperatureUnit = TemperatureUnits.degree;
                        WindSpeedUnit =  WindSpeedUnits.kmh;
                        VisibilityUnit = VisibilityUnits.km;
                        PressureUnit = PressureUnits.hPa_mb;
                        PrecipitationUnit = PrecipitationUnits.mm;
                    }
                    break;
                case WeatherUnits.imperial:
                    {
                        TemperatureUnit = TemperatureUnits.fahrenheit;
                        WindSpeedUnit = WindSpeedUnits.mph;
                        VisibilityUnit = VisibilityUnits.mi;
                        PressureUnit = PressureUnits.hPa_mb;
                        PrecipitationUnit = PrecipitationUnits.inch;
                    }
                    break;
                case WeatherUnits.hybrid:
                    {
                        TemperatureUnit = TemperatureUnits.degree;
                        WindSpeedUnit = WindSpeedUnits.mph;
                        VisibilityUnit = VisibilityUnits.mi;
                        PressureUnit = PressureUnits.hPa_mb;
                        PrecipitationUnit = PrecipitationUnits.mm;
                    }
                    break;
                case WeatherUnits.custom:
                    throw new ArgumentException($"Custom unit must be specified using the {nameof(WeatherUnitSettings)} constructor that accepts individual units.");
                default:
                    throw new NotImplementedException();
            }
            this.Unit = unit;
        }

        public WeatherUnitSettings(TemperatureUnits temperatureUnit,
            WindSpeedUnits windSpeedUnit,
            VisibilityUnits visibilityUnit,
            PressureUnits pressureUnit,
            PrecipitationUnits precipitationUnit)
        {
            this.Unit = WeatherUnits.custom;
            this.TemperatureUnit = temperatureUnit;
            this.WindSpeedUnit = windSpeedUnit;
            this.VisibilityUnit = visibilityUnit;
            this.PressureUnit = pressureUnit;
            this.PrecipitationUnit = precipitationUnit;
        }

        public WeatherUnits Unit { get; private set; }

        public TemperatureUnits TemperatureUnit { get; private set; }

        public WindSpeedUnits WindSpeedUnit { get; private set; }

        public VisibilityUnits VisibilityUnit { get; private set; }

        public PressureUnits PressureUnit { get; private set; }

        public PrecipitationUnits PrecipitationUnit { get; set; }
    }
}
