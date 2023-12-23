using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
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
    public sealed partial class CompassCard : UserControl
    {
        public float? Value
        {
            get { return (float?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(float?), typeof(CompassCard), new PropertyMetadata(null));

        public float? Direction
        {
            get { return (float?)GetValue(DirectionProperty); }
            set
            {
                SetValue(DirectionProperty, value);
                DirectionNormalized = Direction ?? 0;
            }
        }

        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(float?), typeof(CompassCard), new PropertyMetadata(null));

        public float DirectionNormalized
        {
            get { return (float)GetValue(DirectionNormalizedProperty); }
            private set 
            { 
                value = value < 0 ? value + 360 : value;
                value = value / 360f * 100f;
                SetValue(DirectionNormalizedProperty, value); 
            }
        }

        public static readonly DependencyProperty DirectionNormalizedProperty =
            DependencyProperty.Register("DirectionNormalized", typeof(float), typeof(CompassCard), new PropertyMetadata(null));

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(CompassCard), new PropertyMetadata(string.Empty));

        public CompassCard()
        {
            this.InitializeComponent();
        }
    }
}
