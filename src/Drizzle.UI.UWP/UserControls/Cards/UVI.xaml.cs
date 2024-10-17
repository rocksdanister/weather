using Drizzle.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Drizzle.UI.UWP.UserControls.Cards;

// Ref: https://www.who.int/news-room/questions-and-answers/item/radiation-the-ultraviolet-(uv)-index
public sealed partial class UVI : UserControl
{
    /// <summary>
    /// UV Index value
    /// </summary>
    public int? Value
    {
        get
        {
            return (int?)GetValue(ValueProperty);
        }
        set
        {
            SetValue(ValueProperty, value);
            Message = UVIndexString(value);
            Update();
        }
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(int?), typeof(UVI), new PropertyMetadata(null));

    public float[] HourlyValue
    {
        get { return (float[])GetValue(HourlyValueProperty); }
        set
        {
            SetValue(HourlyValueProperty, value);
            if (value is not null && value.Count() > 2)
            {
                MinValue = (int)Math.Round(value.Min());
                MaxValue = (int)Math.Round(value.Max());
            }
            else
            {
                MinValue = null;
                MaxValue = null;
            }
        }
    }

    public static readonly DependencyProperty HourlyValueProperty =
        DependencyProperty.Register("HourlyValue", typeof(float[]), typeof(UVI), new PropertyMetadata(Array.Empty<float>()));

    public int? MinValue
    {
        get { return (int?)GetValue(MinValueProperty); }
        private set { SetValue(MinValueProperty, value); }
    }

    public static readonly DependencyProperty MinValueProperty =
        DependencyProperty.Register("MinValue", typeof(int?), typeof(UVI), new PropertyMetadata(null));

    public int? MaxValue
    {
        get { return (int?)GetValue(MaxValueProperty); }
        private set { SetValue(MaxValueProperty, value); }
    }

    public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register("MaxValue", typeof(int?), typeof(UVI), new PropertyMetadata(null));

    public string Message
    {
        get { return (string)GetValue(MessageProperty); }
        private set { SetValue(MessageProperty, value); }
    }

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register("Message", typeof(string), typeof(UVI), new PropertyMetadata(string.Empty));

    private readonly int margin = 10;

    public UVI()
    {
        this.InitializeComponent();
    }

    private void Update()
    {
        var width = canvas.ActualWidth;
        var height = canvas.ActualHeight;

        if (width == 0 || height == 0)
            return;

        // Center the line
        Canvas.SetLeft(sliderLine, margin);
        sliderLine.Width = width - margin * 2;

        if (Value is not null)
        {
            var percentage = RangePercentage((double)Value, 0, 10);
            var knobLeft = margin - sliderKnob.ActualWidth / 2 + percentage / 100 * sliderLine.ActualWidth;
            Canvas.SetLeft(sliderKnob, knobLeft);
        }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e) => Update();

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) => Update();

    private string UVIndexString(int? index)
    {
        if (index is null)
            return "---";

        var resources = App.Services.GetRequiredService<IResourceService>();
        try
        {
            return index switch
            {
                <= 2 => resources.GetString($"UVIndex0"),
                <= 7 => resources.GetString($"UVIndex1"),
                _ => resources.GetString($"UVIndex2")
            };
        }
        catch
        {
            return "Error";
        }
    }

    private double RangePercentage(double number, double rangeMin, double rangeMax)
    {
        var percentage = ((number - rangeMin) * 100) / (rangeMax - rangeMin);
        if (percentage > 100)
            percentage = 100;
        else if (percentage < 0)
            percentage = 0;

        return percentage;
    }
}
