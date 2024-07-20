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
    public sealed partial class CloudCoverCard : UserControl
    {
        public float? Value
        {
            get { return (float?)GetValue(ValueProperty); }
            set 
            { 
                SetValue(ValueProperty, value);
                ShowClouds(value is not null ? (float)value : 25f);
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(float?), typeof(CloudCoverCard), new PropertyMetadata(null));

        public CloudCoverCard()
        {
            this.InitializeComponent();
        }

        private void ShowClouds(float value)
        {
            clouds100.Visibility = 
                clouds75.Visibility  = 
                clouds50.Visibility = 
                clouds25.Visibility = Visibility.Collapsed;

            if (value >= 90)
                clouds100.Visibility = Visibility.Visible;
            if (value >= 75)
                clouds75.Visibility = Visibility.Visible;
            else if (value >= 50)
                clouds50.Visibility = Visibility.Visible;
            else
                clouds25.Visibility = Visibility.Visible;
        }
    }
}
