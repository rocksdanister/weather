using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Drizzle.Models.Weather;
using Drizzle.Weather.Helpers;

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
                UpdateAnimation();
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(float?), typeof(VisibilityCard), new PropertyMetadata(null));

        public VisibilityUnits Unit
        {
            get { return (VisibilityUnits)GetValue(UnitProperty); }
            set
            {
                SetValue(UnitProperty, value);
                UnitString = value.GetUnitString();
            }
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(VisibilityUnits), typeof(VisibilityCard), new PropertyMetadata(VisibilityUnits.km));

        public string UnitString
        {
            get { return (string)GetValue(UnitStringProperty); }
            private set { SetValue(UnitStringProperty, value); }
        }

        public static readonly DependencyProperty UnitStringProperty =
            DependencyProperty.Register("UnitString", typeof(string), typeof(VisibilityCard), new PropertyMetadata(string.Empty));

        public VisibilityCard()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (Value is null)
                return;

            var minVisibility = 0f;
            var maxVisibility = 25f;
            var minVisualBlur = 0f;
            var maxVisualBlur = 5f;
            var minTextBlur = 0f;
            var maxTextBlur = 1.5f;

            // Normalizer unit
            float visibility = Unit switch
            {
                VisibilityUnits.km => (float)Value,
                VisibilityUnits.mi => (float)WeatherUtil.MiToKm(Value),
                _ => (float)Value,
            };
            // Clamp to range
            visibility = Math.Max(minVisibility, Math.Min(visibility, maxVisibility));
            // Map to blur
            var normalizedVisibility = (visibility - minVisibility) / (maxVisibility - minVisibility);
            var visualBlurAmount = (1f - normalizedVisibility) * (maxVisualBlur - minVisualBlur);
            var textBlurAmount = (1f - normalizedVisibility) * (maxTextBlur - minTextBlur);

            AnimationBlur.Amount = visualBlurAmount;
            TextBlur.Amount = textBlurAmount;
        }
    }
}
