using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.Qweather;

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

