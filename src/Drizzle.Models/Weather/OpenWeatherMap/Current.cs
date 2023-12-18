using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.OpenWeatherMap;

// Generated from json data
// Ref: https://openweathermap.org/current
public class Current
{
    [JsonPropertyName("coord")]
    public Coord Coord { get; set; }

    [JsonPropertyName("weather")]
    public List<WeatherCondition> Weather { get; set; }

    [JsonPropertyName("base")]
    public string Base { get; set; }

    [JsonPropertyName("main")]
    public Main Main { get; set; }

    [JsonPropertyName("visibility")]
    public float Visibility { get; set; }

    [JsonPropertyName("wind")]
    public Wind Wind { get; set; }

    [JsonPropertyName("rain")]
    public Rain Rain { get; set; }

    [JsonPropertyName("clouds")]
    public Clouds Clouds { get; set; }

    [JsonPropertyName("dt")]
    public int Dt { get; set; }

    [JsonPropertyName("sys")]
    public CurrentSys Sys { get; set; }

    [JsonPropertyName("timezone")]
    public int Timezone { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    //[JsonPropertyName("cod")]
    //public int Cod { get; set; }
}
