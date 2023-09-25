using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.OpenMeteo;

/// <summary>
/// Returned by Geocoding Api.
/// </summary>
public class LocationData
{
    /// <summary>
    /// Unique identifier for this exact location
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Location name. Localized following <see cref="GeocodingOptions.Language"/>
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Geographical WGS84 coordinates of this location
    /// </summary>
    public float Latitude { get; set; }

    /// <summary>
    /// Geographical WGS84 coordinates of this location
    /// </summary>
    public float Longitude { get; set; }

    /// <summary>
    /// Elevation above sea level in meters.
    /// </summary>
    public float Elevation { get; set; }

    /// <summary>
    /// Timezone
    /// </summary>
    public string Timezone { get; set; }

    /// <summary>
    /// Type of this location
    /// </summary>
    [JsonPropertyName("feature_code")]
    public string FeatureCode { get; set; }

    /// <summary>
    /// 2-Character country code. "DE" for Germany
    /// </summary>
    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; }

    /// <summary>
    /// Name of the country
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    /// ID for this country
    /// </summary>
    [JsonPropertyName("country_id")]
    public int CountryId { get; set; }

    /// <summary>
    /// Population of this country
    /// </summary>
    public int Population { get; set; }

    /// <summary>
    /// Postcodes for this location
    /// </summary>
    public string[] Postcodes { get; set; }

    /// <summary>
    /// Name of hierarchical administrative area
    /// </summary>
    public string Admin1 { get; set; }

    /// <summary>
    /// Name of hierarchical administrative area
    /// </summary>
    public string Admin2 { get; set; }

    /// <summary>
    /// Name of hierarchical administrative area
    /// </summary>
    public string Admin3 { get; set; }

    /// <summary>
    /// Name of hierarchical administrative area
    /// </summary>
    public string Admin4 { get; set; }

    /// <summary>
    /// Unique ID for the administrative area
    /// </summary>
    [JsonPropertyName("admin1_id")]
    public int Admin1Id { get; set; }

    /// <summary>
    /// Unique ID for the administrative area
    /// </summary>
    [JsonPropertyName("admin2_id")]
    public int Admin2Id { get; set; }

    /// <summary>
    /// Unique ID for the administrative area
    /// </summary>
    [JsonPropertyName("admin3_id")]
    public int Admin3Id { get; set; }

    /// <summary>
    /// Unique ID for the administrative area
    /// </summary>
    [JsonPropertyName("admin4_id")]
    public int Admin4Id { get; set; }
}
