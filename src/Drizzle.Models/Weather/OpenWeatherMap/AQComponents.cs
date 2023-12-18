using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Drizzle.Models.Weather.OpenWeatherMap;

public class AQComponents
{
    [JsonPropertyName("co")]
    public double Co { get; set; }

    [JsonPropertyName("no")]
    public double No { get; set; }

    [JsonPropertyName("no2")]
    public double No2 { get; set; }

    [JsonPropertyName("o3")]
    public double O3 { get; set; }

    [JsonPropertyName("so2")]
    public double So2 { get; set; }

    [JsonPropertyName("pm2_5")]
    public double Pm25 { get; set; }

    [JsonPropertyName("pm10")]
    public double Pm10 { get; set; }

    [JsonPropertyName("nh3")]
    public double Nh3 { get; set; }
}
