using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using ComputeSharp;
using Windows.UI;

namespace Drizzle.UI.UWP.Converters;

public class ComputeSharpFloat3ToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var val = (float3)value;
        return Color.FromArgb(255, (byte)(val.R * 255), (byte)(val.G * 255), (byte)(val.B * 255));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        var val = (Color)value;
        return new float3(val.R/255f, val.G/255f, val.B/255f);
    }
}
