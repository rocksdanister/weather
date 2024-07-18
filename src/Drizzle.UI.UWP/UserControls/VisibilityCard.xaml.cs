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
    public sealed partial class VisibilityCard : UserControl
    {
        public float? Value
        {
            get
            {
                return (float?)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(float?), typeof(CompassCard), new PropertyMetadata(null));

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set
            {
                SetValue(UnitProperty, value);
            }
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(CompassCard), new PropertyMetadata(string.Empty));

        public VisibilityCard()
        {
            this.InitializeComponent();
        }
    }
}
