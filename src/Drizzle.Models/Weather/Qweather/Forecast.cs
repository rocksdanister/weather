using Drizzle.Models.Weather.OpenMeteo;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.Qweather;

// Generated from json data
// Ref: https://dev.qweather.com/docs/api/weather/weather-daily-forecast/
public class Forecast
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("updateTime")]
    public string UpdateTime { get; set; }



    [JsonPropertyName("daily")]
    public List<Daily> Daily { get; set; }


}
