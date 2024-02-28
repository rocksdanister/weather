using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.Weather;

public class ForecastWeather
{
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public string Name { get; set; }
    public string TimeZone { get; set; }
    public DateTime FetchTime { get; set; }
    public WeatherUnitSettings Units { get; set; }
    public IReadOnlyList<DailyWeather> Daily { get; set; }
    /// <summary>
    /// Hour between forecast data
    /// </summary>
    public int ForecastInterval { get; set; }
}
