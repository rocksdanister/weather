using Drizzle.Models.Enums;
using System;
using System.Linq;
using Windows.Globalization;
using Windows.UI.Xaml.Data;

namespace Drizzle.UI.UWP.Converters
{
    public class DateTimeToShortTimeConverter
    {
        public static string FormatTime(DateTime? time, TimeFormats? format)
        {
            var formatString = GetTimeFormatString(format ?? TimeFormats.system);
            return time?.ToString(formatString);
        }

        public static string FormatTime(DateTime time, TimeFormats format)
        {
            var formatString = GetTimeFormatString(format);
            return time.ToString(formatString);
        }

        public static string GraphFormatTime(DateTime time, TimeFormats? format)
        {
            // AM/PM is removed for space reasons.
            string formatString = format switch
            {
                TimeFormats.system => Is24HourClock() ? "HH:mm" : "h:mm",
                TimeFormats.twelveHour => "h:mm",
                TimeFormats.twentyFourHour => "HH:mm",
                _ => Is24HourClock() ? "HH:mm" : "h:mm"
            };
            return time.ToString(formatString);
        }

        private static string GetTimeFormatString(TimeFormats timeFormat)
        {
            return timeFormat switch
            {
                TimeFormats.system => "t",
                TimeFormats.twelveHour => "h:mm tt",
                TimeFormats.twentyFourHour => "HH:mm",
                _ => "t"
            };
        }

        private static bool Is24HourClock()
        {
            var formatter = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime");
            return formatter.Clock == ClockIdentifiers.TwentyFourHour;
        }
    }
}
