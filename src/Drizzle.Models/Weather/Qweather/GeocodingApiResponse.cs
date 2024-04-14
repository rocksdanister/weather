using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.Qweather;


public class GeocodingApiResponse
{

    [JsonPropertyName("location")]
    public LocationData[] location { get; set; }


    [JsonPropertyName("code")]
    public string code { get; set; }
}
