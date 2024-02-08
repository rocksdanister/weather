using GeoTimeZone;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TimeZoneConverter;

namespace Drizzle.Weather.Helpers
{
    public static class TimeUtil
    {
        public static string GetDayName(this DayOfWeek value, bool isAbbreviation = false)
        {
            return isAbbreviation ?
                DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName(value) : DateTimeFormatInfo.CurrentInfo.GetDayName(value);
        }

        public static DateTime? GetLocalTime(string timezone) =>
            GetLocalTime(DateTime.Now, timezone);

        public static DateTime? GetLocalTime(DateTime time, string timezone)
        {
            if (string.IsNullOrEmpty(timezone))
                return null;

            try
            {
                var timeZoneInfo = TZConvert.GetTimeZoneInfo(timezone);
                return TimeZoneInfo.ConvertTime(time, timeZoneInfo);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// True if 6 AM to 6 PM, false otherwise
        /// </summary>
        public static bool IsDaytime()
        {
            var time = DateTime.Now;
            var timeOfDay = time.TimeOfDay;
            var start = new TimeSpan(6, 0, 0);
            var end = new TimeSpan(18, 0, 0);
            return timeOfDay >= start && timeOfDay <= end;
        }

        /// <summary>
        /// True if 6 AM to 6 PM, false otherwise
        /// </summary>
        public static bool IsDaytime(string timezone)
        {
            var localTime = GetLocalTime(timezone) ?? DateTime.Now;
            var timeOfDay = localTime.TimeOfDay;
            var start = new TimeSpan(6, 0, 0);
            var end = new TimeSpan(18, 0, 0);
            return timeOfDay >= start && timeOfDay <= end;
        }

        //ref: https://stackoverflow.com/questions/33639571/get-local-time-based-on-coordinates
        public static string GetTimeZone(double latitude, double longitude)
        {
            string tzIana = TimeZoneLookup.GetTimeZone(latitude, longitude).Result;
            return TZConvert.IanaToWindows(tzIana);
        }

        public static DateTime UnixToDateTime(long unix)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime;
        }

        public static DateTime UnixToLocalDateTime(long unix, string timezone)
        {
            var time = UnixToDateTime(unix);
            return (DateTime)GetLocalTime(time, timezone);
        }

        public static DateTime ISO8601ToDateTime(string iso8601String)
        {
            return DateTime.Parse(iso8601String, CultureInfo.InvariantCulture);
        }
    }
}
