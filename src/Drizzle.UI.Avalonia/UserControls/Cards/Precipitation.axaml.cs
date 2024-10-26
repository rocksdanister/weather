using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Drizzle.Common.Helpers;
using Drizzle.Models.Weather;
using Drizzle.Weather.Helpers;
using System;

namespace Drizzle.UI.Avalonia.UserControls.Cards;

public partial class Precipitation : UserControl
{
    public static readonly StyledProperty<float?> ValueProperty =
    AvaloniaProperty.Register<Precipitation, float?>(nameof(Value));

    public float? Value
    {
        get { return GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    public static readonly StyledProperty<PrecipitationUnits> UnitProperty =
        AvaloniaProperty.Register<Precipitation, PrecipitationUnits>(nameof(Unit), PrecipitationUnits.mm);

    public PrecipitationUnits Unit
    {
        get { return GetValue(UnitProperty); }
        set { SetValue(UnitProperty, value); }
    }

    public static readonly StyledProperty<string> UnitStringProperty =
        AvaloniaProperty.Register<Precipitation, string>(nameof(UnitString), string.Empty);

    public string UnitString
    {
        get { return GetValue(UnitStringProperty); }
        private set { SetValue(UnitStringProperty, value); }
    }

    public static readonly StyledProperty<Thickness> AnimationMarginProperty =
        AvaloniaProperty.Register<Precipitation, Thickness>(nameof(AnimationMargin), new Thickness(0,0,0,-50));

    public Thickness AnimationMargin
    {
        get { return GetValue(AnimationMarginProperty); }
        private set { SetValue(AnimationMarginProperty, value); }
    }

    public Precipitation()
    {
        InitializeComponent();

        ValueProperty.Changed.AddClassHandler<Precipitation>(OnPropertyChanged);
    }

    private static void OnPropertyChanged(Precipitation sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ValueProperty)
        {
            if (sender.Value is null)
                return;

            sender.AnimationMargin = GetWaterMargin((float)sender.Value, sender.Unit);
            // Update unit if required.
            sender.UnitString = sender.Unit.GetUnitString();
        }
    }

    private static Thickness GetWaterMargin(float precipitation, PrecipitationUnits unit)
    {
        // In metric
        var minPrecipitation = 0f;
        var maxPrecipitation = 20f;
        var minMargin = -125f;
        var maxMargin = -25f;

        // Normalize unit
        float metricPrecipitation = unit switch
        {
            PrecipitationUnits.mm => precipitation,
            PrecipitationUnits.inch => (float)WeatherUtil.InchToMm(precipitation)!,
            _ => precipitation,
        };
        // Clamp to range
        metricPrecipitation = Math.Max(minPrecipitation, Math.Min(metricPrecipitation, maxPrecipitation));
        // Map to margin
        var normalizedPrecipitation = (metricPrecipitation - minPrecipitation) / (maxPrecipitation - minPrecipitation);
        var bottomMargin = minMargin + normalizedPrecipitation * (maxMargin - minMargin);

        return new Thickness(0, 0, 0, bottomMargin);
    }
}