using System;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Drizzle.UI.UWP.UserControls
{
    // Ref: https://www.who.int/news-room/questions-and-answers/item/radiation-the-ultraviolet-(uv)-index
    public sealed partial class UVCard : UserControl
    {
        /// <summary>
        /// UV Index value
        /// </summary>
        public int? Value
        {
            get
            {
                return (int?)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
                Message = UVIndexString(value);
                Update();
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int?), typeof(UVCard), new PropertyMetadata(null));

        public float[] HourlyValue
        {
            get { return (float[])GetValue(HourlyValueProperty); }
            set
            {
                SetValue(HourlyValueProperty, value);
                if (value is not null && value.Count() > 2)
                {
                    MinValue = (int)Math.Round(value.Min());
                    MaxValue = (int)Math.Round(value.Max());
                }
                else
                {
                    MinValue = null;
                    MaxValue = null;
                }
            }
        }

        public static readonly DependencyProperty HourlyValueProperty =
            DependencyProperty.Register("HourlyValue", typeof(float[]), typeof(UVCard), new PropertyMetadata(Array.Empty<float>()));

        public int? MinValue
        {
            get { return (int?)GetValue(MinValueProperty); }
            private set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(int?), typeof(UVCard), new PropertyMetadata(null));

        public int? MaxValue
        {
            get { return (int?)GetValue(MaxValueProperty); }
            private set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int?), typeof(UVCard), new PropertyMetadata(null));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            private set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(UVCard), new PropertyMetadata(string.Empty));

        private readonly ResourceLoader resourceLoader;
        private readonly int margin = 10;

        public UVCard()
        {
            this.InitializeComponent();

            if (Windows.UI.Core.CoreWindow.GetForCurrentThread() is not null)
                resourceLoader = ResourceLoader.GetForCurrentView();
        }

        private void Update()
        {
            var width = canvas.ActualWidth;
            var height = canvas.ActualHeight;

            if (width == 0 || height == 0)
                return;

            // Center the line
            Canvas.SetLeft(sliderLine, margin);
            sliderLine.Width = width - margin * 2;

            if (Value is not null)
            {
                // Ref: https://open-meteo.com/en/docs/air-quality-api
                // United States Air Quality Index (AQI) calculated for different particulate matter and gases individually.
                // Ranges from 0-50 (good), 51-100 (moderate), 101-150 (unhealthy for sensitive groups), 151-200 (unhealthy), 201-300 (very unhealthy) and 301-500 (hazardous).
                var percentage = RangePercentage((double)Value, 0, 10);
                var knobLeft = margin - sliderKnob.ActualWidth / 2 + percentage / 100 * sliderLine.ActualWidth;
                Canvas.SetLeft(sliderKnob, knobLeft);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) => Update();

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) => Update();

        private string UVIndexString(int? index)
        {
            if (index is null)
                return "---";

            try
            {
                return index switch
                {
                    <= 2 => resourceLoader?.GetString($"UVIndex0"),
                    <= 7 => resourceLoader?.GetString($"UVIndex1"),
                    _ => resourceLoader?.GetString($"UVIndex2")
                };
            }
            catch
            {
                return "Error";
            }
        }

        private double RangePercentage(double number, double rangeMin, double rangeMax)
        {
            var percentage = ((number - rangeMin) * 100) / (rangeMax - rangeMin);
            if (percentage > 100)
                percentage = 100;
            else if (percentage < 0)
                percentage = 0;

            return percentage;
        }
    }
}
