using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using System.Numerics;

namespace Drizzle.UI.Avalonia.Converters;

public class Vector3ToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = (Vector3)value;
        return Color.FromArgb(255, (byte)(val.X * 255), (byte)(val.Y * 255), (byte)(val.Z * 255));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = (Color)value;
        return new Vector3(val.R / 255f, val.G / 255f, val.B / 255f);
    }
}
