using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Drizzle.Weather.Helpers;

namespace Drizzle.UI.UWP.Converters;

public class DateTimeToDayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
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

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
