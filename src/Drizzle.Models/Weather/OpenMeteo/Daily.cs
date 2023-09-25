using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.Weather.OpenMeteo;

public class Daily
{
    public string[] Time { get; set; }

    /// <summary>
    /// The most severe daily weather condition
    /// </summary>
    /// <value></value>
    public float[] Weathercode { get; set; }

    /// <summary>
    /// Maximum daily temperature at 2 meters above ground
    /// </summary>
    /// <value></value>
    public float[] Temperature_2m_max { get; set; }

    /// <summary>
    /// Minimum daily temperature at 2 meters above ground
    /// </summary>
    /// <value></value>
    public float[] Temperature_2m_min { get; set; }

    /// <summary>
    /// Maximum daily apparent temperature
    /// </summary>
    /// <value></value>
    public float[] Apparent_temperature_max { get; set; }

    /// <summary>
    /// Minimum daily apparent temperature
    /// </summary>
    /// <value></value>
    public float[] Apparent_temperature_min { get; set; }

    /// <summary>
    /// Sunrise time
    /// </summary>
    /// <value></value>
    public string[] Sunrise { get; set; }

    /// <summary>
    /// Sunset time
    /// </summary>
    /// <value></value>
    public string[] Sunset { get; set; }

    /// <summary>
    /// Sum of daily precipitation
    /// </summary>
    /// <value></value>
    public float[] Precipitation_sum { get; set; }

    /// <summary>
    /// Sum of daily rain
    /// </summary>
    /// <value></value>
    public float[] Rain_sum { get; set; }

    /// <summary>
    /// Sum of daily showers
    /// </summary>
    /// <value></value>
    public float[] Showers_sum { get; set; }

    /// <summary>
    /// Sum of daily snowfall
    /// </summary>
    /// <value></value>
    public float[] Snowfall_sum { get; set; }

    /// <summary>
    /// Daily hours with rain
    /// </summary>
    /// <value></value>
    public float[] Precipitation_hours { get; set; }

    /// <summary>
    /// Maximum daily windspeed at 10 meters above ground
    /// </summary>
    /// <value></value>
    public float[] Windspeed_10m_max { get; set; }

    /// <summary>
    /// Maximum daily windgusts at 10 meters above ground
    /// </summary>
    /// <value></value>
    public float[] Windgusts_10m_max { get; set; }

    /// <summary>
    /// Dominant daily wind direction at 10 meters above ground
    /// </summary>
    /// <value></value>
    public float[] Winddirection_10m_dominant { get; set; }

    /// <summary>
    /// Sum of daily solar radiation
    /// </summary>
    /// <value></value>
    public float[] Shortwave_radiation_sum { get; set; }

    /// <summary>
    /// Sum of daily ET₀ Reference Evapotranspiration of a well watered grass field
    /// </summary>
    /// <value></value>
    public float[] Et0_fao_evapotranspiration { get; set; }
}
