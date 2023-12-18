using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Drizzle.Models.Weather.OpenWeatherMap;

public class Main
{
    [JsonPropertyName("temp")]
    public float Temp { get; set; }

    [JsonPropertyName("feels_like")]
    public float FeelsLike { get; set; }

    [JsonPropertyName("temp_min")]
    public float TempMin { get; set; }

    [JsonPropertyName("temp_max")]
    public float TempMax { get; set; }

    [JsonPropertyName("pressure")]
    public float Pressure { get; set; }

    [JsonPropertyName("sea_level")]
    public float SeaLevel { get; set; }

    [JsonPropertyName("grnd_level")]
    public float GrndLevel { get; set; }

    [JsonPropertyName("humidity")]
    public float Humidity { get; set; }

    //[JsonPropertyName("temp_kf")]
    //public float TempKf { get; set; }
}
