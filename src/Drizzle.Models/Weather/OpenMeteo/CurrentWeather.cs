using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.Weather.OpenMeteo;

/// <summary>
/// Api response containing information about current weather conditions
/// </summary>
public class CurrentWeather
{
    public string Time { get; set; }

    /// <summary>
    /// Temperature in <see cref="WeatherForecastOptions.Temperature_Unit"/>
    /// </summary>
    public float Temperature { get; set; }

    /// <summary>
    /// WMO Weather interpretation code.
    /// To get an actual string representation use <see cref="OpenMeteo.OpenMeteoClient.WeathercodeToString(int)"/>
    /// </summary>
    public float Weathercode { get; set; }

    /// <summary>
    /// Windspeed. Unit defined in <see cref="WeatherForecastOptions.Windspeed_Unit"/>
    /// </summary>
    /// <value></value>
    public float Windspeed { get; set; }

    /// <summary>
    /// Wind direction in degrees
    /// </summary>
    public float WindDirection { get; set; }
}
