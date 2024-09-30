using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Drizzle.Common.Helpers;
using Drizzle.Models.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;

namespace Drizzle.UI.Avalonia.UserControls;

public partial class DailyGraph : UserControl
{
    public static readonly StyledProperty<float[]> ValueProperty =
        AvaloniaProperty.Register<DailyGraph, float[]>(nameof(Value), []);

    public float[] Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<string> ValueFormatProperty =
        AvaloniaProperty.Register<DailyGraph, string>(nameof(ValueFormat), "{0}");

    public string ValueFormat
    {
        get => GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    public static readonly StyledProperty<float?> MinValueProperty =
        AvaloniaProperty.Register<DailyGraph, float?>(nameof(MinValue));

    public float? MinValue
    {
        get => GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public static readonly StyledProperty<float?> MaxValueProperty =
        AvaloniaProperty.Register<DailyGraph, float?>(nameof(MaxValue));

    public float? MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public static readonly StyledProperty<DateTime> StartTimeProperty =
        AvaloniaProperty.Register<DailyGraph, DateTime>(nameof(StartTime), DateTime.Today);

    public DateTime StartTime
    {
        get => GetValue(StartTimeProperty);
        set => SetValue(StartTimeProperty, value);
    }

    public static readonly StyledProperty<int> IntervalProperty =
        AvaloniaProperty.Register<DailyGraph, int>(nameof(Interval), 1);

    public int Interval
    {
        get => GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }

    public static readonly StyledProperty<Color> Gradient1Property =
        AvaloniaProperty.Register<DailyGraph, Color>(nameof(Gradient1), Color.FromArgb(100, 255, 255, 255));

    public Color Gradient1
    {
        get => GetValue(Gradient1Property);
        set => SetValue(Gradient1Property, value);
    }

    public static readonly StyledProperty<ObservableCollection<HourlyConditions>> ConditionsProperty =
        AvaloniaProperty.Register<DailyGraph, ObservableCollection<HourlyConditions>>(nameof(Conditions), []);

    public ObservableCollection<HourlyConditions> Conditions
    {
        get => GetValue(ConditionsProperty);
        private set => SetValue(ConditionsProperty, value);
    }

    public static readonly StyledProperty<int[]> WeatherCodesProperty =
        AvaloniaProperty.Register<DailyGraph, int[]>(nameof(WeatherCodes), []);

    public int[] WeatherCodes
    {
        get => GetValue(WeatherCodesProperty);
        set => SetValue(WeatherCodesProperty, value);
    }

    public DailyGraph()
    {
        InitializeComponent();
        ValueProperty.Changed.AddClassHandler<DailyGraph>(OnPropertyChanged);
    }

    public void Draw()
    {
        if (Value == null || Value.Length == 0)
            return;

        var width = MainCanvas.Bounds.Width;
        var height = MainCanvas.Bounds.Height;
        if (!IsValidSize(width) || !IsValidSize(height))
            return;

        // Clear previous drawings
        ClearCanvas();

        var segments = Value.Length;
        // (Label) Let each segment take maximum 42 units width.
        var maxSegments = (int)width / 42;
        // (Label) Number of elements to skip in between to fit the data in the graph
        var step = segments >= maxSegments ? (int)Math.Ceiling((double)segments / maxSegments) : 1;
        Point[] points = new Point[segments + 1];
        // Get min and max values for normalization
        var min = MinValue ?? Value.Min();
        var max = MaxValue ?? Value.Max();
        // Fix graph not showing if all values equal
        max = max == min ? max + 0.1f : max;
        // Prevent last pt on the edge of canvas
        var distanceRightMargin = segments % 2 != 0 && segments > 14 ? -15 : 0;
        // Spacing between x - points
        var distanceMultiplier = (width + distanceRightMargin) / segments;
        // Weather condition
        var iconOffset = new Vector2(5f, (float)height - 63f);
        var iconPoints = WeatherCodes.Length != 0 ? new List<Point>() : null;

        // Calculate points for the graph
        for (int i = 0; i < segments; i++)
        {
            var x = i * distanceMultiplier;
            var normalizedY = MathUtil.ConvertToRange(min, max, 0.25f, 0.55f, Value[i]);
            var y = height - (normalizedY * height);
            points[i] = new Point(x, y);

            if (i % step == 0)
                iconPoints?.Add(new Point(x, 0) + iconOffset);
        }
        // Make the graph reach end of canvas
        points[segments] = new Point((segments + 1) * distanceMultiplier, points[segments - 1].Y);

        DrawArea(MainCanvas, points, height, [new(Gradient1, 0), new(Colors.Transparent, 1)]);
        DrawGraphOutline(MainCanvas, points, Gradient1);
        DrawLabels(MainCanvas, points, Value, StartTime, step, ValueFormat, Interval, Color.FromArgb(100, 255, 255, 255), Colors.White);
        DrawHorizontalLine(MainCanvas, width, height - 30, Color.FromArgb(50, 255, 255, 255));
        DrawWeatherCodes(iconPoints, step);
    }

    private void ClearCanvas()
    {
        MainCanvas.Children.Clear();
    }

    private static void DrawArea(Canvas canvas, Point[] points, double height, GradientStops gradientStops)
    {
        var pathFigure = new PathFigure { StartPoint = new Point(0, height) };
        var polyLineSegment = new PolyLineSegment { Points = points };

        pathFigure.Segments!.Add(polyLineSegment);
        var lineSegment = new LineSegment
        {
            Point = new Point(canvas.Bounds.Width, height)
        };
        pathFigure.Segments.Add(lineSegment);

        var pathGeometry = new PathGeometry();
        pathGeometry.Figures!.Add(pathFigure);

        var gradientBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops = gradientStops
        };

        var path = new Path
        {
            Data = pathGeometry,
            Fill = gradientBrush
        };

        canvas.Children.Add(path);
    }

    private static void DrawGraphOutline(Canvas canvas, Point[] points, Color outlineColor)
    {
        var polyline = new Polyline
        {
            Points = new Points(points),
            Stroke = new SolidColorBrush(outlineColor),
            StrokeThickness = 1
        };

        canvas.Children.Add(polyline);
    }

    private static void DrawHorizontalLine(
        Canvas canvas,
        double width,
        double height,
        Color strokeColor)
    {
        var line = new Line
        {
            StartPoint = new Point(0, height),
            EndPoint = new Point(width, height),
            Stroke = new SolidColorBrush(strokeColor),
            StrokeThickness = 1
        };

        canvas.Children.Add(line);
    }

    private static void DrawLabels(Canvas canvas,
        Point[] points,
        float[] value,
        DateTime startTime,
        int step,
        string valueFormat,
        int interval,
        Color xLabelColor, 
        Color yLabelColor)
    {
        var height = canvas.Bounds.Height;
        for (int i = 0; i < points.Length - 1; i++)
        {
            if (i % step != 0) 
                continue;

            // Draw time label
            var x = points[i].X + 5;
            var timeLabel = startTime.AddHours(interval * i).ToString("HH:mm");
            var text = CreateTextBlock(timeLabel, xLabelColor);
            Canvas.SetLeft(text, x);
            Canvas.SetTop(text, height - 20);
            canvas.Children.Add(text);

            // Draw value label
            var formattedValue = string.Format(valueFormat, value[i]);
            var valueText = CreateTextBlock(formattedValue, yLabelColor);
            Canvas.SetLeft(valueText, x + 8);
            Canvas.SetTop(valueText, points[i].Y - 25);
            canvas.Children.Add(valueText);
        }
    }

    private void DrawWeatherCodes(List<Point>? pts, int step)
    {
        Conditions.Clear();
        if (WeatherCodes.Length == 0 || pts is null)
            return;

        for (int i = 0; i < pts.Count; i++)
        {
            var pt = pts[i];
            var weatherIndex = i * step;
            if (weatherIndex >= WeatherCodes.Length)
                break;

            var hour = StartTime.AddHours(weatherIndex * Interval).Hour;
            Conditions.Add(new(WeatherCodes[weatherIndex], hour >= 6 && hour < 18, (float)pt.X, (float)pt.Y));
        }
    }

    private void OnPropertyChanged(DailyGraph sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ValueProperty)
            Draw();
    }

    private void UserControl_Loaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        Draw();
    }

    private void UserControl_SizeChanged(object? sender, global::Avalonia.Controls.SizeChangedEventArgs e)
    {
        Draw();
    }

    private static TextBlock CreateTextBlock(string text, Color color)
    {
        return new TextBlock
        {
            Text = text,
            Foreground = new SolidColorBrush(color)
        };
    }

    private static bool IsValidSize(double size) => !(size == 0 || double.IsNaN(size));
}