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
    public sealed partial class HumidityCard : UserControl
    {
        public float? Humidity
        {
            get { return (float?)GetValue(HumidityProperty); }
            set 
            { 
                SetValue(HumidityProperty, value); 
            }
        }

        public static readonly DependencyProperty HumidityProperty =
            DependencyProperty.Register("Humidity", typeof(float?), typeof(HumidityCard), new PropertyMetadata(null));

        public HumidityCard()
        {
            this.InitializeComponent();
        }
    }
}
