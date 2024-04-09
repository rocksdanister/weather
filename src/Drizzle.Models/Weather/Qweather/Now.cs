using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.Qweather;

// Generated from json data
// Ref: https://dev.qweather.com/docs/api/weather/weather-now/
public class Now
{
    [JsonPropertyName("obsTime")]
    public string ObsTime { get; set; }

    [JsonPropertyName("temp")]
    public string Temp { get; set; }

    [JsonPropertyName("feelsLike")]
    public string FeelsLike { get; set; }

    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("wind360")]
    public string Wind360 { get; set; }

    [JsonPropertyName("windDir")]
    public string WindDir { get; set; }

    [JsonPropertyName("windScale")]
    public string WindScale { get; set; }

    [JsonPropertyName("windSpeed")]
    public string WindSpeed { get; set; }

    [JsonPropertyName("humidity")]
    public string Humidity { get; set; }

    [JsonPropertyName("precip")]
    public string Precip { get; set; }

    [JsonPropertyName("pressure")]
    public string Pressure { get; set; }

    [JsonPropertyName("vis")]
    public string Vis { get; set; }

    [JsonPropertyName("cloud")]
    public string Cloud { get; set; }

    [JsonPropertyName("dew")]
    public string Dew { get; set; }
}
