using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.Qweather;

// Generated from json data
// Ref:https://dev.qweather.com/docs/api/air/air-daily-forecast/
public class AirQualityForecast
{
    [JsonPropertyName("code")]
    public string Code { get; set; }


    [JsonPropertyName("updateTime")]
    public string UpdateTime { get; set; }

    [JsonPropertyName("daily")]
    public List<AirQualityDaily> List { get; set; }
}
