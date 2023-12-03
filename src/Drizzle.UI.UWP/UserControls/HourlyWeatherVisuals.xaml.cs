using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class HourlyWeatherVisuals : UserControl
    {
        public HourlyData[] Value
        {
            get { return (HourlyData[])GetValue(ValueProperty); }
            private set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(HourlyData[]), typeof(HourlyWeatherVisuals), new PropertyMetadata(Array.Empty<HourlyData>()));

        public int[] WeatherCodes
        {
            get { return (int[])GetValue(WeatherCodesProperty); }
            set
            {
                var stepValue = value?.Where((value, index) => (index % Step) == 0).ToArray();
                SetValue(WeatherCodesProperty, stepValue);

                Value = [];
                if (stepValue is not null)
                {
                    Value = new HourlyData[stepValue.Length];
                    for (int i = 0; i < stepValue.Length; i++)
                        Value[i] = new HourlyData(stepValue[i], IsDaytime);
                }
            }
        }

        public static readonly DependencyProperty WeatherCodesProperty =
            DependencyProperty.Register("WeatherCodes", typeof(int[]), typeof(HourlyWeatherVisuals), new PropertyMetadata(Array.Empty<int>()));

        public int Step
        {
            get { return (int)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register("Step", typeof(int), typeof(HourlyWeatherVisuals), new PropertyMetadata(1));

        public bool IsDaytime
        {
            get { return (bool)GetValue(IsDaytimeProperty); }
            set { SetValue(IsDaytimeProperty, value); }
        }

        public static readonly DependencyProperty IsDaytimeProperty =
            DependencyProperty.Register("IsDaytime", typeof(bool), typeof(HourlyWeatherVisuals), new PropertyMetadata(false));

        public HourlyWeatherVisuals()
        {
            this.InitializeComponent();
        }
    }

    public class HourlyData
    {
        public int WeatherCode { get; set; }
        public bool IsDaytime { get; set; }

        public HourlyData(int weatherCode, bool isDaytime)
        {
            this.WeatherCode = weatherCode;
            this.IsDaytime = isDaytime;
        }
    }
}
