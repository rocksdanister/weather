using System;
using Windows.UI.Xaml.Data;

namespace Drizzle.UI.UWP.Converters
{
    public class DateTimeToShortTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
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

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
