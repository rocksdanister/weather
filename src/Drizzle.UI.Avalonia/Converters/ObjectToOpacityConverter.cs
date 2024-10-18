using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Drizzle.UI.Avalonia.Converters;

internal class ObjectToOpacityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return 0;
        else return 1;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
