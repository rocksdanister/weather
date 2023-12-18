using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Drizzle.Models.Weather.OpenWeatherMap;

public class AQMain
{
    [JsonPropertyName("aqi")]
    public int Aqi { get; set; }
}
