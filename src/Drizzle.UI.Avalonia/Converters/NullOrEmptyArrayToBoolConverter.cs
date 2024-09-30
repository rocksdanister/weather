using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Drizzle.UI.Avalonia.Converters;

public class NullOrEmptyArrayToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !(value is null || (value as Array).Length == 0) ^ (parameter as string ?? string.Empty).Equals("Reverse");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
