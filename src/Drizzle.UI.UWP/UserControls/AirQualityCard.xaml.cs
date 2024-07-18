using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Drizzle.UI.UWP.UserControls
{
    public sealed partial class AirQualityCard : UserControl
    {
        /// <summary>
        /// Air quality index (US AQI) value
        /// </summary>
        public int? Value
        {
            get { 
                return (int?)GetValue(ValueProperty); 
            }
            set 
            { 
                SetValue(ValueProperty, value);
                Message = AirQualityString(value);
                Update();
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int?), typeof(AirQualityCard), new PropertyMetadata(null));

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
            DependencyProperty.Register("HourlyValue", typeof(float[]), typeof(AirQualityCard), new PropertyMetadata(Array.Empty<float>()));

        public int? MinValue
        {
            get { return (int?)GetValue(MinValueProperty); }
            private set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(int?), typeof(AirQualityCard), new PropertyMetadata(null));

        public int? MaxValue
        {
            get { return (int?)GetValue(MaxValueProperty); }
            private set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int?), typeof(AirQualityCard), new PropertyMetadata(null));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            private set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(AirQualityCard), new PropertyMetadata(string.Empty));

        private readonly int margin = 10;
        private readonly ResourceLoader resourceLoader;

        public AirQualityCard()
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
            sliderLine.Width = width - margin*2;

            if (Value is not null)
            {
                // Ref: https://open-meteo.com/en/docs/air-quality-api
                // United States Air Quality Index (AQI) calculated for different particulate matter and gases individually.
                // Ranges from 0-50 (good), 51-100 (moderate), 101-150 (unhealthy for sensitive groups), 151-200 (unhealthy), 201-300 (very unhealthy) and 301-500 (hazardous).
                var percentage = RangePercentage((double)Value, 0, 500);
                var knobLeft = margin - sliderKnob.ActualWidth / 2 + percentage / 100 * sliderLine.ActualWidth;
                Canvas.SetLeft(sliderKnob, knobLeft);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) => Update();

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) => Update();

        private string AirQualityString(int? aqi)
        {
            if (aqi is null)
                return "---";

            try
            {
                return aqi switch
                {
                    <= 50 => resourceLoader?.GetString($"AirQualityIndex0"),
                    <= 100 => resourceLoader?.GetString($"AirQualityIndex1"),
                    <= 150 => resourceLoader?.GetString($"AirQualityIndex2"),
                    <= 200 => resourceLoader?.GetString($"AirQualityIndex3"),
                    <= 300 => resourceLoader?.GetString($"AirQualityIndex4"),
                    <= 500 => resourceLoader?.GetString($"AirQualityIndex5"),
                    _ => "---"
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
