using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.Weather.OpenMeteo;

public class Daily_Units
{
    public string Time { get; set; }
    public string Weathercode { get; set; }
    public string Temperature_2m_max { get; set; }
    public string Temperature_2m_min { get; set; }
    public string Apparent_temperature_max { get; set; }
    public string Apparent_temperature_min { get; set; }
    public string Sunrise { get; set; }
    public string Sunset { get; set; }
    public string Precipitation_sum { get; set; }
    public string Rain_sum { get; set; }
    public string Showers_sum { get; set; }
    public string Snowfall_sum { get; set; }
    public string Precipitation_hours { get; set; }
    public string Windspeed_10m_max { get; set; }
    public string Windgusts_10m_max { get; set; }
    public string Winddirection_10m_dominant { get; set; }
    public string Shortwave_radiation_sum { get; set; }
    public string Et0_fao_evapotranspiration { get; set; }
}
