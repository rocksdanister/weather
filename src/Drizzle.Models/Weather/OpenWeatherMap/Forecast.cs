using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.OpenWeatherMap;

// Generated from json data
// Ref: https://openweathermap.org/forecast5
public class Forecast
{
    //[JsonPropertyName("cod")]
    //public string Cod { get; set; }

    //[JsonPropertyName("message")]
    //public int Message { get; set; }

    [JsonPropertyName("cnt")]
    public int Cnt { get; set; }

    [JsonPropertyName("list")]
    public List<ForecastList> List { get; set; }

    [JsonPropertyName("city")]
    public City City { get; set; }
}
