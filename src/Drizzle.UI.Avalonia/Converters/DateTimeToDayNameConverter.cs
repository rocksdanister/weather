using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Drizzle.Weather.Helpers;

namespace Drizzle.UI.Avalonia.Converters;

public class DateTimeToDayNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            return ((DateTime)value).DayOfWeek.GetDayName((parameter as string ?? string.Empty).Equals("Short"));
        }
        catch
        {
            return "Error";
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
