using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Drizzle.Models.Weather.OpenWeatherMap;

public class AQList
{
    [JsonPropertyName("dt")]
    public int Dt { get; set; }

    [JsonPropertyName("main")]
    public AQMain Main { get; set; }

    [JsonPropertyName("components")]
    public AQComponents Components { get; set; }
}
