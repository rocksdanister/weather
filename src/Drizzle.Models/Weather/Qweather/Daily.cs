using Drizzle.Models.Weather.OpenWeatherMap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace Drizzle.Models.Weather.Qweather;
// Generated from json data
// Ref: https://dev.qweather.com/docs/api/weather/weather-daily-forecast/
public class Daily
{
    [JsonPropertyName("fxDate")]
    public string FxDate { get; set; }

    [JsonPropertyName("sunrise")]
    public string Sunrise { get; set; }

    [JsonPropertyName("sunset")]
    public string Sunset { get; set; }

    [JsonPropertyName("moonrise")]
    public string Moonrise { get; set; }

    [JsonPropertyName("moonset")]
    public string Moonset { get; set; }

    [JsonPropertyName("moonPhase")]
    public string MoonPhase { get; set; }

    [JsonPropertyName("moonPhaseIcon")]
    public string MoonPhaseIcon { get; set; }

    [JsonPropertyName("tempMax")]
    public string TempMax { get; set; }

    [JsonPropertyName("tempMin")]
    public string TempMin { get; set; }

    [JsonPropertyName("iconDay")]
    public string IconDay { get; set; }

    [JsonPropertyName("textDay")]
    public string TextDay { get; set; }

    [JsonPropertyName("iconNight")]
    public string IconNight { get; set; }

    [JsonPropertyName("textNight")]
    public string TextNight { get; set; }

    [JsonPropertyName("wind360Day")]
    public string Wind360Day { get; set; }

    [JsonPropertyName("windDirDay")]
    public string WindDirDay { get; set; }

    [JsonPropertyName("windScaleDay")]
    public string WindScaleDay { get; set; }

    [JsonPropertyName("windSpeedDay")]
    public string WindSpeedDay { get; set; }

    [JsonPropertyName("wind360Night")]
    public string Wind360Night { get; set; }

    [JsonPropertyName("windDirNight")]
    public string WindDirNight { get; set; }

    [JsonPropertyName("windScaleNight")]
    public string WindScaleNight { get; set; }

    [JsonPropertyName("windSpeedNight")]
    public string WindSpeedNight { get; set; }

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

    [JsonPropertyName("uvIndex")]
    public string UvIndex { get; set; }
}

