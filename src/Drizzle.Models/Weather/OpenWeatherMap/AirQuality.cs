using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.OpenWeatherMap;

// Generated from json data
// Ref: https://openweathermap.org/api/air-pollution
public class AirQuality
{
    [JsonPropertyName("coord")]
    public Coord Coord { get; set; }

    [JsonPropertyName("list")]
    public List<AQList> List { get; set; }
}
