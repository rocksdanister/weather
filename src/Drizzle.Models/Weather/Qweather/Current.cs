using Drizzle.Models.Weather.OpenMeteo;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.Qweather;

// Generated from json data
// Ref: https://dev.qweather.com/docs/api/weather/weather-now/
public class Current
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("updateTime")]
    public string UpdateTime { get; set; }



    [JsonPropertyName("now")]
    public Now Now { get; set; }


}
