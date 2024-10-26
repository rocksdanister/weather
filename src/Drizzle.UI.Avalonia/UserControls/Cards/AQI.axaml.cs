using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Drizzle.Common.Helpers;
using Drizzle.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Drizzle.UI.Avalonia.UserControls.Cards;

public partial class AQI : UserControl
{
    /// <summary>
    /// Air quality index (US AQI) value
    /// </summary>
    public int? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<int?> ValueProperty =
        AvaloniaProperty.Register<AQI, int?>(nameof(Value));

    /// <summary>
    /// Hourly air quality values.
    /// </summary>
    public float[] HourlyValue
    {
        get => GetValue(HourlyValueProperty);
        set => SetValue(HourlyValueProperty, value);
    }

    public static readonly StyledProperty<float[]> HourlyValueProperty =
        AvaloniaProperty.Register<AQI, float[]>(nameof(HourlyValue));

    /// <summary>
    /// Minimum air quality index value.
    /// </summary>
    public int? MinValue
    {
        get => GetValue(MinValueProperty);
        private set => SetValue(MinValueProperty, value);
    }

    public static readonly StyledProperty<int?> MinValueProperty =
        AvaloniaProperty.Register<AQI, int?>(nameof(MinValue));

    /// <summary>
    /// Maximum air quality index value.
    /// </summary>
    public int? MaxValue
    {
        get => GetValue(MaxValueProperty);
        private set => SetValue(MaxValueProperty, value);
    }

    public static readonly StyledProperty<int?> MaxValueProperty =
        AvaloniaProperty.Register<AQI, int?>(nameof(MaxValue));

    /// <summary>
    /// Air quality description.
    /// </summary>
    public string Message
    {
        get => GetValue(MessageProperty);
        private set => SetValue(MessageProperty, value);
    }

    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<AQI, string>(nameof(Message));

    public AQI()
    {
        InitializeComponent();

        ValueProperty.Changed.AddClassHandler<AQI>(OnPropertyChanged);
        HourlyValueProperty.Changed.AddClassHandler<AQI>(OnPropertyChanged);
    }

    private void Update()
    {
        var margin = 10;
        var width = SliderCanvas.Bounds.Width;
        var height = SliderCanvas.Bounds.Height;
        var knobWidth = SliderKnob.Bounds.Width;

        if (!IsValidSize(width) || !IsValidSize(height) || !IsValidSize(knobWidth))
            return;

        // Center the line
        Canvas.SetLeft(SliderLine, margin);
        SliderLine.Width = width - margin * 2;

        if (Value is not null)
        {
            // Ref: https://open-meteo.com/en/docs/air-quality-api
            // United States Air Quality Index (AQI) calculated for different particulate matter and gases individually.
            // Ranges from 0-50 (good), 51-100 (moderate), 101-150 (unhealthy for sensitive groups), 151-200 (unhealthy), 201-300 (very unhealthy) and 301-500 (hazardous).
            var percentage = MathUtil.RangePercentage((double)Value, 0, 500);
            var knobLeft = margin - knobWidth / 2 + percentage / 100 * SliderLine.Bounds.Width;
            Canvas.SetLeft(SliderKnob, knobLeft);
        }
    }

    private void UserControl_Loaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        Update();
    }

    private void UserControl_SizeChanged(object? sender, global::Avalonia.Controls.SizeChangedEventArgs e)
    {
        Update();
    }

    private static void OnPropertyChanged(AQI sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ValueProperty)
        {
            sender.Message = AirQualityString(sender.Value) ?? "---";
            sender.Update();
        }
        else if (e.Property == HourlyValueProperty)
        {
            if (sender.HourlyValue is not null && sender.HourlyValue.Length > 2)
            {
                sender.MinValue = (int)Math.Round(sender.HourlyValue.Min());
                sender.MaxValue = (int)Math.Round(sender.HourlyValue.Max());
            }
            else
            {
                sender.MinValue = null;
                sender.MaxValue = null;
            }
        }
    }

    private static string? AirQualityString(int? aqi)
    {
        if (aqi is null)
            return "---";

        var resources = App.Services.GetRequiredService<IResourceService>();
        try
        {
            return aqi switch
            {
                <= 50 => resources.GetString($"AirQualityIndex0"),
                <= 100 => resources.GetString($"AirQualityIndex1"),
                <= 150 => resources.GetString($"AirQualityIndex2"),
                <= 200 => resources.GetString($"AirQualityIndex3"),
                <= 300 => resources.GetString($"AirQualityIndex4"),
                <= 500 => resources.GetString($"AirQualityIndex5"),
                _ => "---"
            };
        }
        catch
        {
            return "Error";
        }
    }

    private static bool IsValidSize(double size) => !(size == 0 || double.IsNaN(size));
}