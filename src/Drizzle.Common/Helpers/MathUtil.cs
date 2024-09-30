using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Common.Helpers
{
    public static class MathUtil
    {
        public static float Lerp(float start, float target, float by) => start * (1 - by) + target * by;

        public static double RangePercentage(double number, double rangeMin, double rangeMax)
        {
            var percentage = ((number - rangeMin) * 100) / (rangeMax - rangeMin);
            if (percentage > 100)
                percentage = 100;
            else if (percentage < 0)
                percentage = 0;

            return percentage;
        }

        public static double DegreeToRadians(double degree) => degree * Math.PI / 180;

        public static float NormalizeAngle(float degree) => degree < 0 ? 360 + degree : degree;

        public static float ConvertToRange(float oldStart, float oldEnd, float newStart, float newEnd, float value)
        {
            var scale = (newEnd - newStart) / (oldEnd - oldStart);
            return newStart + ((value - oldStart) * scale);
        }

        // Credit: https://gist.github.com/adrianstevens/8163205
        public static string DegreeToCardinalString(float degree)
        {
            var cardinals = new string[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };
            var value = NormalizeAngle(degree);
            var index = (int)Math.Round(value % 360 / 45);
            return cardinals[index];
        }
    }
}
