using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.OpenWeatherMap;

// Generated from json data
// Ref: https://openweathermap.org/api/geocoding-api
public class LocationData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("local_names")]
    public Dictionary<string, string> LocalNames { get; set; }

    [JsonPropertyName("lat")]
    public float Lat { get; set; }

    [JsonPropertyName("lon")]
    public float Lon { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }
}
