using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Drizzle.Models.Weather.OpenWeatherMap;

public class Rain
{
    [JsonPropertyName("1h")]
    public float _1h { get; set; }

    [JsonPropertyName("3h")]
    public float _3h { get; set; }
}
