using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.Qweather;

// Generated from json data
// Ref:https://dev.qweather.com/docs/api/air/air-now/
public class AirQualityCurrent
{
    [JsonPropertyName("code")]
    public string Code { get; set; }


    [JsonPropertyName("updateTime")]
    public string UpdateTime { get; set; }

    [JsonPropertyName("now")]
    public AirQualityNow Now { get; set; }

   

}


public class AirQualityNow
{
    [JsonPropertyName("pubTime")]
    public string PubTime { get; set; }

    [JsonPropertyName("aqi")]
    public string Aqi { get; set; }

    [JsonPropertyName("level")]
    public string Level { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("primary")]
    public string Primary { get; set; }

    [JsonPropertyName("pm10")]
    public string Pm10 { get; set; }

    [JsonPropertyName("pm2p5")]
    public string Pm2P5 { get; set; }

    [JsonPropertyName("no2")]
    public string No2 { get; set; }

    [JsonPropertyName("so2")]
    public string So2 { get; set; }

    [JsonPropertyName("co")]
    public string Co { get; set; }

    [JsonPropertyName("o3")]
    public string O3 { get; set; }
}