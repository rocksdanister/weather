using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Drizzle.Models.Weather.OpenWeatherMap;

public class Coord
{
    [JsonPropertyName("lon")]
    public float Lon { get; set; }

    [JsonPropertyName("lat")]
    public float Lat { get; set; }
}
