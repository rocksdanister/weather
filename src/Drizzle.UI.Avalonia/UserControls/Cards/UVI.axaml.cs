using Avalonia;
using Avalonia.Controls;
using Drizzle.Common.Helpers;
using Drizzle.UI.Avalonia.Helpers;
using System;
using System.Linq;

namespace Drizzle.UI.Avalonia.UserControls.Cards;

public partial class UVI : UserControl
{
    public int? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<int?> ValueProperty =
        AvaloniaProperty.Register<UVI, int?>(nameof(Value));

    public float[] HourlyValue
    {
        get => GetValue(HourlyValueProperty);
        set => SetValue(HourlyValueProperty, value);
    }

    public static readonly StyledProperty<float[]> HourlyValueProperty =
        AvaloniaProperty.Register<UVI, float[]>(nameof(HourlyValue), []);

    public int? MinValue
    {
        get => GetValue(MinValueProperty);
        private set => SetValue(MinValueProperty, value);
    }

    public static readonly StyledProperty<int?> MinValueProperty =
        AvaloniaProperty.Register<UVI, int?>(nameof(MinValue));

    public int? MaxValue
    {
        get => GetValue(MaxValueProperty);
        private set => SetValue(MaxValueProperty, value);
    }

    public static readonly StyledProperty<int?> MaxValueProperty =
        AvaloniaProperty.Register<UVI, int?>(nameof(MaxValue));

    public string Message
    {
        get => GetValue(MessageProperty);
        private set => SetValue(MessageProperty, value);
    }

    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<UVI, string>(nameof(Message));

    public UVI()
    {
        InitializeComponent();

        ValueProperty.Changed.AddClassHandler<UVI>(OnPropertyChanged);
        HourlyValueProperty.Changed.AddClassHandler<UVI>(OnPropertyChanged);
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
            var percentage = MathUtil.RangePercentage((double)Value, 0, 10);
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

    private static void OnPropertyChanged(UVI sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ValueProperty)
        {
            sender.Message = UVIndexString(sender.Value);
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

    private static string UVIndexString(int? index)
    {
        if (index is null)
            return "---";

        try
        {
            return index switch
            {
                <= 2 => ResourceUtil.GetString($"UVIndex0"),
                <= 7 => ResourceUtil.GetString($"UVIndex1"),
                _ => ResourceUtil.GetString($"UVIndex2")
            } ?? "---";
        }
        catch
        {
            return "Error";
        }
    }

    private static bool IsValidSize(double size) => !(size == 0 || double.IsNaN(size));
}