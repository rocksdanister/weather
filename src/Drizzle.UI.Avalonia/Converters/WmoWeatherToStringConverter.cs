using Avalonia.Data.Converters;
using Drizzle.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;

namespace Drizzle.UI.Avalonia.Converters;

public class WmoWeatherToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            return App.Services.GetRequiredService<IResourceService>().GetString($"WmoWeatherString{(int)value}");
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
