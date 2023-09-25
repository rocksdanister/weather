using System.Text.Json.Serialization;

namespace Drizzle.Models.Weather.OpenMeteo;

/// <summary>
/// Air Quality Api response
/// </summary>
public class AirQuality
{
    /// <summary>
    /// WGS84 of the center of the weather grid-cell which was used to generate this forecast.
    /// </summary>
    [JsonPropertyName("latitude")]
    public float Latitude { get; set; }

    /// <summary>
    /// WGS84 of the center of the weather grid-cell which was used to generate this forecast.
    /// </summary>
    [JsonPropertyName("longitude")]
    public float Longitude { get; set; }

    /// <summary>
    /// Generation time of the weather forecast in milliseconds.
    /// </summary>
    [JsonPropertyName("generationtime_ms")]
    public float GenerationTime { get; set; }

    /// <summary>
    /// Timezone offset from <see cref="AirQualityOptions.Timezone"/>
    /// </summary>
    [JsonPropertyName("utc_offset_seconds")]
    public int UtcOffset { get; set; }

    /// <summary>
    /// Timezone identifier
    /// </summary>
    [JsonPropertyName("timezone")]
    public string Timezone { get; set; }

    /// <summary>
    /// Timezone abbreviation
    /// </summary>
    [JsonPropertyName("timezone_abbreviation")]
    public string TimezoneAbbreviation { get; set; }

    /// <summary>
    /// Returned data for each selected variable in <see cref="AirQualityOptions.Hourly"/>
    /// </summary>
    [JsonPropertyName("hourly")]
    public HourlyValues Hourly { get; set; }

    /// <summary>
    /// The unit for each selected variable in <see cref="Hourly"/>
    /// </summary>
    [JsonPropertyName("hourly_units")]
    public HourlyUnits Hourly_Units { get; set; }

    public class HourlyUnits
    {
        public string Time { get; set; }
        public string Pm10 { get; set; }
        public string Pm2_5 { get; set; }
        public string Carbon_monoxide { get; set; }
        public string Nitrogen_dioxide { get; set; }
        public string Sulphur_dioxide { get; set; }
        public string Ozone { get; set; }
        public string Aerosol_optical_depth { get; set; }
        public string Dust { get; set; }
        public string Uv_index { get; set; }
        public string Uv_index_clear_sky { get; set; }
        public string Alder_pollen { get; set; }
        public string Birch_pollen { get; set; }
        public string Grass_pollen { get; set; }
        public string Mugwort_pollen { get; set; }
        public string Olive_pollen { get; set; }
        public string Ragweed_pollen { get; set; }
        public string European_aqi { get; set; }
        public string European_aqi_pm2_5 { get; set; }
        public string European_aqi_pm10 { get; set; }
        public string European_aqi_no2 { get; set; }
        public string European_aqi_o3 { get; set; }
        public string European_aqi_so2 { get; set; }
        public string Us_aqi { get; set; }
        public string Us_aqi_pm2_5 { get; set; }
        public string Us_aqi_pm10 { get; set; }
        public string Us_aqi_no2 { get; set; }
        public string Us_aqi_o3 { get; set; }
        public string Us_aqi_so2 { get; set; }
        public string Us_aqi_co { get; set; }
    }

    public class HourlyValues
    {
        public string[] Time { get; set; }
        public float?[] Pm10 { get; set; }
        public float?[] Pm2_5 { get; set; }
        public float?[] Carbon_monoxide { get; set; }
        public float?[] Nitrogen_dioxide { get; set; }
        public float?[] Sulphur_dioxide { get; set; }
        public float?[] Ozone { get; set; }
        public float?[] Aerosol_optical_depth { get; set; }
        public float?[] Dust { get; set; }
        public float?[] Uv_index { get; set; }
        public float?[] Uv_index_clear_sky { get; set; }

        /// <summary>
        /// Only available in Europe during pollen season with 4 days forecast
        /// </summary>
        public float?[] Alder_pollen { get; set; }

        /// <summary>
        /// Only available in Europe during pollen season with 4 days forecast
        /// </summary>
        public float?[] Birch_pollen { get; set; }

        /// <summary>
        /// Only available in Europe during pollen season with 4 days forecast
        /// </summary>
        public float?[] Grass_pollen { get; set; }

        /// <summary>
        /// Only available in Europe during pollen season with 4 days forecast
        /// </summary>
        public float?[] Mugwort_pollen { get; set; }

        /// <summary>
        /// Only available in Europe during pollen season with 4 days forecast
        /// </summary>
        public float?[] Olive_pollen { get; set; }

        /// <summary>
        /// Only available in Europe during pollen season with 4 days forecast
        /// </summary>
        public float?[] Ragweed_pollen { get; set; }
        public float?[] European_aqi { get; set; }
        public float?[] European_aqi_pm2_5 { get; set; }
        public float?[] European_aqi_pm10 { get; set; }
        public float?[] European_aqi_no2 { get; set; }
        public float?[] European_aqi_o3 { get; set; }
        public float?[] European_aqi_so2 { get; set; }
        public float?[] Us_aqi { get; set; }
        public float?[] Us_aqi_pm2_5 { get; set; }
        public float?[] Us_aqi_pm10 { get; set; }
        public float?[] Us_aqi_no2 { get; set; }
        public float?[] Us_aqi_o3 { get; set; }
        public float?[] Us_aqi_so2 { get; set; }
        public float?[] Us_aqi_co { get; set; }
    }
}