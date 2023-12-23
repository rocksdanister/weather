using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.Weather;

public class DailyWeather
{
    public int WeatherCode { get; set; }

    public float? TemperatureMin { get; set; }

    public float? TemperatureMax { get; set; }

    public float? Temperature { get; set; }

    public float? ApparentTemperatureMax { get; set; }

    public float? ApparentTemperatureMin { get; set; }

    public float? ApparentTemperature { get; set; }

    public int? Humidity { get; set; }

    public float? Visibility { get; set; }

    public float? Pressure { get; set; }

    public float? DewPoint { get; set; }

    public float? WindSpeed { get; set; }

    public float? GustSpeed { get; set; }

    public float? WindDirection { get; set; }

    public DateTime? Sunrise { get; set; }

    public DateTime? Sunset { get; set; }

    public int[] HourlyWeatherCode { get; set; }

    public float[] HourlyTemperature { get; set; }

    public float[] HourlyVisibility { get; set; }

    public float[] HourlyPressure { get; set; }

    public float[] HourlyHumidity { get; set; }

    public float[] HourlyWindSpeed { get; set; }

    public DateTime Date { get; set; }

    //public IReadOnlyList<DateTime> HourlyTime { get; set; }
}
