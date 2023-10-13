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
        public int[] WeatherCodes
        {
            get { return (int[])GetValue(WeatherCodesProperty); }
            set
            {
                var stepValue = value?.Where((value, index) => (index % Step) == 0).ToArray();
                SetValue(WeatherCodesProperty, stepValue);
            }
        }

        public static readonly DependencyProperty WeatherCodesProperty =
            DependencyProperty.Register("WeatherCodes", typeof(int[]), typeof(DailyGraph), new PropertyMetadata(Array.Empty<int>()));

        public int Step
        {
            get { return (int)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register("Step", typeof(int), typeof(DailyGraph), new PropertyMetadata(1));

        public HourlyWeatherVisuals()
        {
            this.InitializeComponent();
        }
    }
}
