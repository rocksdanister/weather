using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.Qweather;

// Generated from json data
// Ref: https://dev.qweather.com/docs/api/geoapi/city-lookup/
public class LocationData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("lat")]
    public string Lat { get; set; }

    [JsonPropertyName("lon")]
    public string Lon { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("adm2")]
    public string Adm2 { get; set; }

    [JsonPropertyName("adm1")]
    public string Adm1 { get; set; }

}
