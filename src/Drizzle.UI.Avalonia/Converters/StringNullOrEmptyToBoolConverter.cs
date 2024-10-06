using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Drizzle.UI.Avalonia.Converters;

public class StringNullOrEmptyToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return string.IsNullOrEmpty(stringValue) ^ (parameter as string ?? string.Empty).Equals("Reverse");
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
