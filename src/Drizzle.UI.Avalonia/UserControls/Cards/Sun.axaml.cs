using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Drizzle.Common.Helpers;
using Drizzle.Weather.Helpers;
using System;

namespace Drizzle.UI.Avalonia.UserControls.Cards;

public partial class Sun : UserControl
{
    public static readonly StyledProperty<DateTime?> SunriseProperty =
    AvaloniaProperty.Register<Sun, DateTime?>(nameof(Sunrise), DateTime.Today.AddHours(6));

    public DateTime? Sunrise
    {
        get => GetValue(SunriseProperty);
        set => SetValue(SunriseProperty, value);
    }

    public static readonly StyledProperty<DateTime?> SunsetProperty =
        AvaloniaProperty.Register<Sun, DateTime?>(nameof(Sunset), DateTime.Today.AddHours(18));

    public DateTime? Sunset
    {
        get => GetValue(SunsetProperty);
        set => SetValue(SunsetProperty, value);
    }

    public static readonly StyledProperty<string> TimeZoneProperty =
        AvaloniaProperty.Register<Sun, string>(nameof(TimeZone), null);

    public string TimeZone
    {
        get => GetValue(TimeZoneProperty);
        set => SetValue(TimeZoneProperty, value);
    }

    private readonly DispatcherTimer dispatcherTimer = new();

    public Sun()
    {
        InitializeComponent();

        SunriseProperty.Changed.AddClassHandler<Sun>(OnPropertyChanged);
        SunsetProperty.Changed.AddClassHandler<Sun>(OnPropertyChanged);
        TimeZoneProperty.Changed.AddClassHandler<Sun>(OnPropertyChanged);

        dispatcherTimer.Tick += DispatcherTimer_Tick;
        dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
        dispatcherTimer.Start();
    }

    private void Update()
    {
        // Place the sun start position if no data available
        DateTime sunrise = Sunrise ?? DateTime.Today.AddDays(1),
            sunset = Sunset ?? DateTime.Today.AddDays(2);

        var width = MainCanvas.Bounds.Width;
        var height = MainCanvas.Bounds.Height;

        SunLine.Width = width;
        SunLine.Height = height;

        Canvas.SetLeft(SunLine, 0);
        Canvas.SetTop(SunLine, height / 2 - 25);

        // Sun position
        var localTime = TimeUtil.GetLocalTime(TimeZone) ?? DateTime.Now;
        var timePercent = GetTimePercent(localTime, sunrise, sunset);
        var angle = timePercent / 100 * 180;
        var radians = MathUtil.DegreeToRadians(180 + angle);

        // Ellipse
        // Ref: https://www.mathopenref.com/coordparamellipse.html
        // x = acos(theta), y = bsin(theta) where (a, b) radius (x, y)
        double x = width / 2 * Math.Cos(radians) + width / 2;
        double y = height / 2 * Math.Sin(radians) + height - 25;

        // Offset the picture size
        var offsetX = x - SunAnimation.Bounds.Width / 2;
        var offsetY = y + SunAnimation.Bounds.Height / 2;
        Canvas.SetLeft(SunAnimation, offsetX);
        Canvas.SetTop(SunAnimation, offsetY);
    }

    private static void OnPropertyChanged(Sun sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == SunriseProperty || e.Property == SunsetProperty || e.Property == TimeZoneProperty)
            sender.Update();
    }

    private void DispatcherTimer_Tick(object? sender, object e)
    {
        Update();
    }

    private void UserControl_Loaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        Update();
    }

    private void UserControl_Unloaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        dispatcherTimer.Tick -= DispatcherTimer_Tick;
        dispatcherTimer.Stop();
    }

    private void UserControl_SizeChanged(object? sender, global::Avalonia.Controls.SizeChangedEventArgs e)
    {
        Update();
    }

    /// <summary>
    /// Calculates percentage of given time based on starting and ending time
    /// </summary>
    private static double GetTimePercent(DateTime time, DateTime start, DateTime end)
    {
        // Check if the time is within range
        if (time >= start && time <= end)
        {
            TimeSpan total = end - start;
            TimeSpan elapsed = time - start;

            return elapsed.TotalSeconds / total.TotalSeconds * 100;
        }
        else
        {
            return time < start ? 0 : 100;
        }
    }
}