using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Drizzle.UI.UWP.Converters
{
    public class TimeToIsDaytimeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var time = ((DateTime)value);
            var isDaytime = time.Hour >= 6 && time.Hour < 18;
            return isDaytime ^ (parameter as string ?? string.Empty).Equals("Reverse") ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
