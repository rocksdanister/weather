﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.Qweather;

/// <summary>
/// Geocoding API response
/// </summary>
public class GeocodingApiResponse
{
    /// <summary>
    /// Array of found locations
    /// </summary>
    [JsonPropertyName("location")]
    public LocationData[] Locations { get; set; }

    /// <summary>
    /// Status Code
    /// </summary>
    [JsonPropertyName("code")]
    public int code { get; set; }
}
