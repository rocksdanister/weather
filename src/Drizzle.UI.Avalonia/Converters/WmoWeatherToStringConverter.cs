using Avalonia.Data.Converters;
using Drizzle.UI.Avalonia.Helpers;
using System;
using System.Globalization;

namespace Drizzle.UI.Avalonia.Converters;

public class WmoWeatherToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            return ResourceUtil.GetString($"WmoWeatherString{(int)value}");
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
