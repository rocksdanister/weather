using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.Qweather;

// Generated from json data
// Ref: https://dev.qweather.com/docs/api/weather/weather-daily-forecast/
public class AirQualityDaily
{
    [JsonPropertyName("fxDate")]
    public string FxDate { get; set; }

    [JsonPropertyName("aqi")]
    public string Aqi { get; set; }

    [JsonPropertyName("level")]
    public string Level { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("primary")]
    public string Primary { get; set; }
}

