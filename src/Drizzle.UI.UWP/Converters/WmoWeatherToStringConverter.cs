using Drizzle.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace Drizzle.UI.UWP.Converters
{
    public class WmoWeatherToStringConverter : IValueConverter
    {
        private readonly ResourceLoader resourceLoader;

        public WmoWeatherToStringConverter()
        {
            if (Windows.UI.Core.CoreWindow.GetForCurrentThread() is not null)
                resourceLoader = ResourceLoader.GetForCurrentView();
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                return resourceLoader?.GetString($"WmoWeatherString{value}");
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
