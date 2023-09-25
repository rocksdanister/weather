using System;
using System.Collections.Generic;
using System.Text;
using static Drizzle.Models.Weather.OpenMeteo.AirQualityOptions;

namespace Drizzle.Models.Weather.OpenMeteo;

public class WeatherForecastOptions
{
    /// <summary>
    /// Geographical WGS84 coordinate of the location
    /// </summary>
    public float Latitude { get; set; }

    /// <summary>
    /// Geographical WGS84 coordinate of the location
    /// </summary>
    public float Longitude { get; set; }

    /// <summary>
    /// Default is "celsius". Use "fahrenheit" to convert temperature to fahrenheit
    /// </summary>
    public TemperatureUnitType Temperature_Unit { get; set; }

    /// <summary>
    /// Default is "kmh". Other options: "ms", "mph", "kn"
    /// </summary>
    public WindspeedUnitType Windspeed_Unit { get; set; }

    /// <summary>
    /// Default is "mm". Other options: "inch"
    /// </summary>
    public PrecipitationUnitType Precipitation_Unit { get; set; }

    /// <summary>
    /// Default is "land". Other options: "sea": prefers grid-cells on sea level, "nearest": nearest grid cell
    /// </summary>
    public CellSelectionType Cell_Selection { get; set; }

    /// <summary>
    /// Default is "GMT". Any time zone name from the time zone database is supported.
    /// </summary>
    public string Timezone { get; set; }

    public HourlyOptions Hourly { get { return _hourly; } set { if (value != null) _hourly = value; } }
    public DailyOptions Daily { get { return _daily; } set { if (value != null) _daily = value; } }
    public WeatherModelOptions Models { get { return _models; } set { if (value != null) _models = value; } }

    /// <summary>
    /// Default is "true".
    /// Include current weather conditions in API response.
    /// </summary>
    public bool Current_Weather { get; set; }

    /// <summary>
    /// Default is "iso8601". Other options: "unixtime". 
    /// Please note that all timestamp are in GMT+0!
    /// See https://open-meteo.com/en/docs for more info
    /// </summary>
    public TimeformatType Timeformat { get; set; }

    /// <summary>
    /// Default is "0". Other options: "1", "2"
    /// </summary>
    /// <value></value>
    public int Past_Days { get; set; }

    /// <summary>
    /// The time interval to get weather data. A day must be specified as an ISO8601 date (e.g. 2022-06-30).
    /// (yyyy-mm-dd)
    /// https://open-meteo.com/en/docs
    /// </summary>
    public string Start_date { get; set; }

    /// <summary>
    /// The time interval to get weather data. A day must be specified as an ISO8601 date (e.g. 2022-06-30).
    /// (yyyy-mm-dd)
    /// https://open-meteo.com/en/docs
    /// </summary>
    public string End_date { get; set; }

    private HourlyOptions _hourly = new HourlyOptions();
    private DailyOptions _daily = new DailyOptions();
    private WeatherModelOptions _models = new WeatherModelOptions();

    public WeatherForecastOptions(float latitude, float longitude, TemperatureUnitType temperature_Unit, WindspeedUnitType windspeed_Unit, PrecipitationUnitType precipitation_Unit, string timezone, HourlyOptions hourly, DailyOptions daily, bool current_Weather, TimeformatType timeformat, int past_Days, string start_date, string end_date, WeatherModelOptions models, CellSelectionType cell_selection)
    {
        Latitude = latitude;
        Longitude = longitude;
        Temperature_Unit = temperature_Unit;
        Windspeed_Unit = windspeed_Unit;
        Precipitation_Unit = precipitation_Unit;
        Timezone = timezone;

        if (hourly != null)
            Hourly = hourly;
        if (daily != null)
            Daily = daily;
        if (models != null)
            Models = models;

        Current_Weather = current_Weather;
        Timeformat = timeformat;
        Past_Days = past_Days;
        Start_date = start_date;
        End_date = end_date;
        Cell_Selection = cell_selection;
    }
    public WeatherForecastOptions(float latitude, float longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        Temperature_Unit = TemperatureUnitType.celsius;
        Windspeed_Unit = WindspeedUnitType.kmh;
        Precipitation_Unit = PrecipitationUnitType.mm;
        Timeformat = TimeformatType.iso8601;
        Cell_Selection = CellSelectionType.land;
        Timezone = "GMT";
        Current_Weather = true;
        Start_date = string.Empty;
        End_date = string.Empty;
    }
    public WeatherForecastOptions()
    {
        Latitude = 0f;
        Longitude = 0f;
        Temperature_Unit = TemperatureUnitType.celsius;
        Windspeed_Unit = WindspeedUnitType.kmh;
        Precipitation_Unit = PrecipitationUnitType.mm;
        Timeformat = TimeformatType.iso8601;
        Cell_Selection = CellSelectionType.land;
        Timezone = "GMT";
        Current_Weather = true;
        Start_date = string.Empty;
        End_date = string.Empty;
    }
}

public enum TemperatureUnitType
{
    celsius,
    fahrenheit
}

public enum WindspeedUnitType
{
    kmh,
    ms,
    mph,
    kn
}

public enum PrecipitationUnitType
{
    mm,
    inch
}

public enum TimeformatType
{
    iso8601,
    unixtime
}

public enum CellSelectionType
{
    land,
    sea,
    nearest
}
