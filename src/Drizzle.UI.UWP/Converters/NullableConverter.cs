using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.UI.UWP.Converters
{
    public static class NullableConverter
    {
        public static double ConvertFloat(float? value) => value ?? 0;
    }
}
