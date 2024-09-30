using Drizzle.Common.Helpers;
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
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return "---";

            return MathUtil.DegreeToCardinalString((float)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
