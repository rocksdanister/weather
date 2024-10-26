using Drizzle.Models.Weather;
using Drizzle.Weather.Helpers;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Drizzle.UI.UWP.UserControls.Cards;

public sealed partial class Precipitation : UserControl
{
    public float? Value
    {
        get { return (float?)GetValue(ValueProperty); }
        set
        {
            SetValue(ValueProperty, value);
            UpdateAnimation();
        }
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(float?), typeof(Precipitation), new PropertyMetadata(null));

    public PrecipitationUnits Unit
    {
        get { return (PrecipitationUnits)GetValue(UnitProperty); }
        set 
        { 
            SetValue(UnitProperty, value);
            UnitString = value.GetUnitString();
        }
    }

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register("Unit", typeof(PrecipitationUnits), typeof(Precipitation), new PropertyMetadata(PrecipitationUnits.mm));

    public string UnitString
    {
        get { return (string)GetValue(UnitStringProperty); }
        private set { SetValue(UnitStringProperty, value); }
    }

    public static readonly DependencyProperty UnitStringProperty =
        DependencyProperty.Register("UnitString", typeof(string), typeof(Precipitation), new PropertyMetadata(string.Empty));

    public Precipitation()
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

        var MinPrecipitation = 0f;
        var MaxPrecipitation = 20f;
        var MinMargin = -125f;
        var MaxMargin = -25f;

        // Normalize unit
        float precipitation = Unit switch
        {
            PrecipitationUnits.mm => (float)Value,
            PrecipitationUnits.inch => (float)WeatherUtil.InchToMm(Value),
            _ => (float)Value,
        };
        // Clamp to range
        precipitation = Math.Max(MinPrecipitation, Math.Min(precipitation, MaxPrecipitation));
        // Map to margin
        var normalizedPrecipitation = (precipitation - MinPrecipitation) / (MaxPrecipitation - MinPrecipitation);
        var bottomMargin = MinMargin + normalizedPrecipitation * (MaxMargin - MinMargin);

        WaveAnimation.Margin = new Thickness(0, 0, 0, bottomMargin);
    }
}
