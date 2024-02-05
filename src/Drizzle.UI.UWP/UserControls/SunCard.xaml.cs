using Drizzle.Weather.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//TODO: Make it work for different local from system

namespace Drizzle.UI.UWP.UserControls
{
    public sealed partial class SunCard : UserControl
    {
        public DateTime? Sunrise
        {
            get { 
                return (DateTime?)GetValue(SunriseProperty); 
            }
            set 
            {
                SetValue(SunriseProperty, value);
                Update();
            }
        }

        public static readonly DependencyProperty SunriseProperty =
            DependencyProperty.Register("Sunrise", typeof(DateTime?), typeof(SunCard), new PropertyMetadata(DateTime.Today.AddHours(6)));

        public DateTime? Sunset
        {
            get { 
                return (DateTime?)GetValue(SunsetProperty); 
            }
            set 
            { 
                SetValue(SunsetProperty, value);
                Update();
            }
        }

        public static readonly DependencyProperty SunsetProperty =
            DependencyProperty.Register("Sunset", typeof(DateTime?), typeof(SunCard), new PropertyMetadata(DateTime.Today.AddHours(18)));

        public string TimeZone
        {
            get { 
                return (string)GetValue(TimeZoneProperty); 
            }
            set 
            { 
                SetValue(TimeZoneProperty, value);
                Update();
            }
        }

        public static readonly DependencyProperty TimeZoneProperty =
            DependencyProperty.Register("TimeZone", typeof(string), typeof(SunCard), new PropertyMetadata(null));

        private readonly DispatcherTimer dispatcherTimer = new();

        public SunCard()
        {
            this.InitializeComponent();

            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 10, 0);
            dispatcherTimer.Start();
        }

        private void Update()
        {
            // Place the sun start position if no data available
            DateTime sunrise = Sunrise ?? DateTime.Today.AddDays(1), 
                sunset = Sunset ?? DateTime.Today.AddDays(2);

            var width = canvas.ActualWidth;
            var height = canvas.ActualHeight;

            sunLine.Width = width;
            sunLine.Height = height;

            Canvas.SetLeft(sunLine, 0);
            Canvas.SetTop(sunLine, height/2 - 25);

            // Sun position
            var localTime = TimeUtil.GetLocalTime(TimeZone) ?? DateTime.Now;
            var timePercent = GetTimePercent(localTime, sunrise, sunset);
            var angle = timePercent/100 * 180;
            var radians = DegreeToRadians(180 + angle);

            // Ellipse
            // Ref: https://www.mathopenref.com/coordparamellipse.html
            // x = acos(theta), y = bsin(theta) where (a, b) radius (x, y)
            double x = width / 2 * Math.Cos(radians) + width / 2;
            double y = height / 2 * Math.Sin(radians) + height - 25;

            // Offset the picture size
            var offsetX = x - sun.ActualWidth / 2;
            var offsetY = y + sun.ActualHeight / 2;
            Canvas.SetLeft(sun, offsetX);
            Canvas.SetTop(sun, offsetY);
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            Update();
        }

        /// <summary>
        /// Calculates percentage of given time based on starting and ending time
        /// </summary>
        private static double GetTimePercent(DateTime time, DateTime start, DateTime end)
        {
            // Check if the time is within range
            if (time >= start && time <= end)
            {
                TimeSpan total = end - start;
                TimeSpan elapsed = time - start;

                return elapsed.TotalSeconds / total.TotalSeconds * 100;
            }
            else
            {
                return time < start ? 0 : 100;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Tick -= DispatcherTimer_Tick;
            dispatcherTimer.Stop();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) => Update();

        private static double DegreeToRadians(double degree) => degree * Math.PI / 180;
    }
}
