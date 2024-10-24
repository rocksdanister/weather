using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Drizzle.Weather.Helpers;
using System;
using System.Linq;

namespace Drizzle.UI.Avalonia.UserControls;

public partial class Clock : UserControl
{

    public static readonly StyledProperty<string> TimeZoneProperty =
    AvaloniaProperty.Register<Clock, string>(nameof(TimeZone), null);

    public string TimeZone
    {
        get => GetValue(TimeZoneProperty);
        set => SetValue(TimeZoneProperty, value);
    }

    public static readonly StyledProperty<DateTime?> TimeProperty =
        AvaloniaProperty.Register<Clock, DateTime?>(nameof(Time), null);

    public DateTime? Time
    {
        get => GetValue(TimeProperty);
        private set => SetValue(TimeProperty, value);
    }

    private readonly DispatcherTimer dispatcherTimer = new();

    public Clock()
    {
        InitializeComponent();

        TimeZoneProperty.Changed.AddClassHandler<Clock>(OnPropertyChanged);

        dispatcherTimer.Tick += DispatcherTimer_Tick;
        dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
        dispatcherTimer.Start();
    }

    private void Update()
    {
        Time = TimeUtil.GetLocalTime(TimeZone);
    }

    private static void OnPropertyChanged(Clock sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TimeZoneProperty)
            sender.Update();
    }

    private void DispatcherTimer_Tick(object? sender, EventArgs e)
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
}