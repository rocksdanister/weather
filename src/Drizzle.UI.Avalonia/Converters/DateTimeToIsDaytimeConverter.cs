using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Drizzle.UI.Avalonia.Converters;

public class DateTimeToIsDaytimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var time = ((DateTime)value);
        var isDaytime = time.Hour >= 6 && time.Hour < 18;
        return isDaytime ^ (parameter as string ?? string.Empty).Equals("Reverse");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
