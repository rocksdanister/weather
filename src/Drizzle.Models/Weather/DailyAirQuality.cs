using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.Weather;

public class DailyAirQuality
{
    public int? AQI { get; set; }

    public float? UV { get; set; }

    public DateTime StartTime { get; set; }

    public float[] HourlyAQI { get; set; }

    public float[] HourlyUV { get; set; }
}
