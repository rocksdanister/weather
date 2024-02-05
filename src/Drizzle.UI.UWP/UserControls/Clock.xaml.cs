using Drizzle.Weather.Helpers;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Drizzle.UI.UWP.UserControls
{
    public sealed partial class Clock : UserControl
    {
        public string TimeZone
        {
            get
            {
                return (string)GetValue(TimeZoneProperty);
            }
            set
            {
                SetValue(TimeZoneProperty, value);
                Update();
            }
        }

        public static readonly DependencyProperty TimeZoneProperty =
            DependencyProperty.Register("TimeZone", typeof(string), typeof(Clock), new PropertyMetadata(null));

        public DateTime? Time
        {
            get => (DateTime?)GetValue(TimeProperty);
            private set => SetValue(TimeProperty, value);
        }

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(DateTime?), typeof(Clock), new PropertyMetadata(null));

        private readonly DispatcherTimer dispatcherTimer = new();

        public Clock()
        {
            this.InitializeComponent();

            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
            dispatcherTimer.Start();
        }

        private void Update()
        {
            Time = TimeUtil.GetLocalTime(TimeZone);
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            Update();
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
    }
}
