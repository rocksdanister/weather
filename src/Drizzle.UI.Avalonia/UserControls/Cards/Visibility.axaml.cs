using Avalonia;
using Avalonia.Controls;
using Drizzle.Models.Weather;
using Drizzle.Weather.Helpers;
using System;

namespace Drizzle.UI.Avalonia.UserControls.Cards;

public partial class Visibility : UserControl
{
    public static readonly StyledProperty<float?> ValueProperty =
    AvaloniaProperty.Register<Visibility, float?>(nameof(Value));

    public float? Value
    {
        get { return GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    public static readonly StyledProperty<VisibilityUnits> UnitProperty =
        AvaloniaProperty.Register<Visibility, VisibilityUnits>(nameof(Unit), VisibilityUnits.km);

    public VisibilityUnits Unit
    {
        get { return GetValue(UnitProperty); }
        set { SetValue(UnitProperty, value); }
    }

    public static readonly StyledProperty<string> UnitStringProperty =
        AvaloniaProperty.Register<Visibility, string>(nameof(UnitString), string.Empty);

    public string UnitString
    {
        get { return GetValue(UnitStringProperty); }
        private set { SetValue(UnitStringProperty, value); }
    }

    public static readonly StyledProperty<float> AnimationBlurAmountProperty =
        AvaloniaProperty.Register<Visibility, float>(nameof(AnimationBlurAmount), 0f);

    public float AnimationBlurAmount
    {
        get { return GetValue(AnimationBlurAmountProperty); }
        private set { SetValue(AnimationBlurAmountProperty, value); }
    }

    public static readonly StyledProperty<float> TextBlurAmountProperty =
        AvaloniaProperty.Register<Visibility, float>(nameof(TextBlurAmount), 0f);

    public float TextBlurAmount
    {
        get { return GetValue(TextBlurAmountProperty); }
        private set { SetValue(TextBlurAmountProperty, value); }
    }

    public Visibility()
    {
        InitializeComponent();

        ValueProperty.Changed.AddClassHandler<Visibility>(OnPropertyChanged);
    }

    private static void OnPropertyChanged(Visibility sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ValueProperty)
        {
            if (sender.Value is null)
                return;

            var (animationBlur, textBlur) = GetBlur((float)sender.Value, sender.Unit);
            sender.AnimationBlurAmount = animationBlur;
            sender.TextBlurAmount = textBlur;
            // Update unit if required.
            sender.UnitString = sender.Unit.GetUnitString();
        }
    }

    private static (float animationBlur, float textBlur) GetBlur(float visibility, VisibilityUnits unit)
    {
        // In metric
        var minVisibility = 0f;
        var maxVisibility = 25f;
        var minVisualBlur = 0f;
        var maxVisualBlur = 5f;
        var minTextBlur = 0f;
        var maxTextBlur = 1.5f;

        // Normalized unit
        float metricVisibility = unit switch
        {
            VisibilityUnits.km => visibility,
            VisibilityUnits.mi => (float)WeatherUtil.MiToKm(visibility)!,
            _ => visibility,
        };
        // Clamp to range
        metricVisibility = Math.Max(minVisibility, Math.Min(metricVisibility, maxVisibility));
        // Map to blur
        var normalizedVisibility = (metricVisibility - minVisibility) / (maxVisibility - minVisibility);
        var animationBlur = (1f - normalizedVisibility) * (maxVisualBlur - minVisualBlur);
        var textBlur = (1f - normalizedVisibility) * (maxTextBlur - minTextBlur);

        return (animationBlur, textBlur);
    }
}