using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Drizzle.UI.Avalonia.Converters;

public class DateTimeToShortTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            return value is null ? "---" : ((DateTime)value).ToShortTimeString();
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
