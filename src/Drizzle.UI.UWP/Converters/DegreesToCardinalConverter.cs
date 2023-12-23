using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Drizzle.UI.UWP.Converters
{
    // Credit: https://gist.github.com/adrianstevens/8163205
    public class DegreesToCardinalConverter : IValueConverter
    {
        private readonly string[] cardinals = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return "---";

            float degree = (float)value < 0 ? 360 + (float)value : (float)value;
            int index = (int)Math.Round(degree % 360 / 45);
            return cardinals[index];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
