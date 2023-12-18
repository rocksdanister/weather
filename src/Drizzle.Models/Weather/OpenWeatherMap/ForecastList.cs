using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Drizzle.Models.Weather.OpenWeatherMap;

public class ForecastList
{
    [JsonPropertyName("dt")]
    public int Dt { get; set; }

    [JsonPropertyName("main")]
    public Main Main { get; set; }

    [JsonPropertyName("weather")]
    public List<WeatherCondition> Weather { get; set; }

    [JsonPropertyName("clouds")]
    public Clouds Clouds { get; set; }

    [JsonPropertyName("wind")]
    public Wind Wind { get; set; }

    [JsonPropertyName("visibility")]
    public float Visibility { get; set; }

    [JsonPropertyName("pop")]
    public float Pop { get; set; }

    [JsonPropertyName("rain")]
    public Rain Rain { get; set; }

    [JsonPropertyName("sys")]
    public ForecastSys Sys { get; set; }

    [JsonPropertyName("dt_txt")]
    public string DtTxt { get; set; }
}
