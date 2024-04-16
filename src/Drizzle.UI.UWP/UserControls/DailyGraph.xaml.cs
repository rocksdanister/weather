using Drizzle.Models.UserControls;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Drizzle.UI.UWP.UserControls
{
    /// <summary>
    /// 24hr daily graph.
    /// X-axis time, Y-axis data.
    /// </summary>
    public sealed partial class DailyGraph : UserControl
    {
        public float[] Value
        {
            get {
                return (float[])GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
                canvas?.Invalidate();
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(float[]), typeof(DailyGraph), new PropertyMetadata(Array.Empty<float>()));

        public string ValueFormat
        {
            get { return (string)GetValue(ValueFormatProperty); }
            set
            {
                SetValue(ValueFormatProperty, value);
                canvas?.Invalidate();
            }
        }

        public static readonly DependencyProperty ValueFormatProperty =
            DependencyProperty.Register("ValueFormat", typeof(string), typeof(DailyGraph), new PropertyMetadata("{0}"));

        public float? MinValue
        {
            get { return (float?)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(float?), typeof(DailyGraph), new PropertyMetadata(null));

        public float? MaxValue
        {
            get { return (float?)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(float?), typeof(DailyGraph), new PropertyMetadata(null));

        public DateTime StartTime
        {
            get { return (DateTime)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register("StartTime", typeof(DateTime), typeof(DailyGraph), new PropertyMetadata(DateTime.Today));

        /// <summary>
        /// Hours between the data points
        /// </summary>
        public int Interval
        {
            get { return (int)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(int), typeof(DailyGraph), new PropertyMetadata(1));

        // Issue: https://github.com/MicrosoftDocs/windows-dev-docs/issues/237
        public Color Gradient1
        {
            get { return (Color)GetValue(Gradient1Property); }
            set 
            { 
                if (value != Gradient1)
                {
                    SetValue(Gradient1Property, value);
                }
            }
        }

        public static readonly DependencyProperty Gradient1Property =
            DependencyProperty.Register("Gradient1", typeof(Color), typeof(DailyGraph), new PropertyMetadata(Color.FromArgb(100, 255, 255, 255)));

        public HourlyConditions[] Conditions
        {
            get { return (HourlyConditions[])GetValue(ConditionsProperty); }
            private set { SetValue(ConditionsProperty, value); }
        }

        public static readonly DependencyProperty ConditionsProperty =
            DependencyProperty.Register("Conditions", typeof(HourlyConditions[]), typeof(DailyGraph), new PropertyMetadata(Array.Empty<HourlyConditions>()));

        public int[] WeatherCodes
        {
            get { return (int[])GetValue(WeatherCodesProperty); }
            set { SetValue(WeatherCodesProperty, value); }
        }

        public static readonly DependencyProperty WeatherCodesProperty =
            DependencyProperty.Register("WeatherCodes", typeof(int[]), typeof(DailyGraph), new PropertyMetadata(Array.Empty<int>()));

        private Color labelColor = Color.FromArgb(100, 255, 255, 255);
        private Color lineColor = Color.FromArgb(50, 255, 255, 255);
        private Color textColor = Colors.White;

        public DailyGraph()
        {
            this.InitializeComponent();
        }

        private void Canvas_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args) =>
            args.TrackAsyncAction(Canvas_CreateResourcesAsync(sender).AsAsyncAction());

        private async Task Canvas_CreateResourcesAsync(CanvasControl sender)
        {
            // Nothing
        }

        // Based on: https://github.com/xyzzer/Win2DChartSample
        private void Canvas_OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            args.DrawingSession.Clear(Colors.Transparent);

            if (Value is null || !Value.Any())
            {
                // Clear conditions if graph is empty.
                DrawWeatherCodes(null, 1);
                return;
            }

            // Avoid allocation if condition is empty.
            var iconPts = WeatherCodes.Any() ? new List<Vector2>() : null;
            // Top - bottom gradient.
            using var brush = new CanvasLinearGradientBrush(canvas, [
                new CanvasGradientStop { Position = 0.0f, Color = Gradient1 },
                new CanvasGradientStop { Position = 1.0f, Color = Colors.Transparent }
            ]);
            brush.StartPoint = new Vector2(0, 0);
            brush.EndPoint = new Vector2(0, (float)canvas.ActualHeight);

            int movingAverageRange = 4;
            // Maximum number of elements to draw
            int segments = Value.Count();
            var min = MinValue != null ? MinValue : Value.Min();
            var max = MaxValue != null ? MaxValue : Value.Max();
            // Fix graph not showing if all values equal
            max = max == min ? max + 0.1f : max;
            var normalizedData = Value.Select(x => ConvertToRange((float)min, (float)max, 0.25f, 0.55f, x)).ToArray();
            // (Label) Let each segment take maximum 42 units width.
            var maxSegments = (int)canvas.ActualWidth / 42;
            // (Label) Number of elements to skip in between to fit the data in the graph
            var step = segments >= maxSegments ? (int)Math.Ceiling((double)segments / maxSegments) : 1;

            var graphMargin = new Vector2(5f, 0f);
            var xLabelOffset = new Vector2(0f, (float)canvas.ActualHeight - 25f);
            var yLabelOffset = new Vector2(8f, -25f); // Time string takes ~16 units width, so half to center.
            var iconOffset = new Vector2(0f, (float)canvas.ActualHeight - 63f);
            var graphStartPos = new Vector2(0, (float)(canvas.ActualHeight * (1 - normalizedData[0])));

            int previousRangeLeft = 0;
            int previousRangeRight = 0;
            // Prevent last pt on the edge of canvas
            var distanceRightMargin = segments % 2 != 0 && segments > 14 ? -15 : 0;
            var distanceMultiplier = (canvas.ActualWidth + distanceRightMargin) / segments;

            // Starting pt
            using var cpb = new CanvasPathBuilder(args.DrawingSession);
            cpb.BeginFigure(graphStartPos);
            DrawText(GetElapsedTimeString(StartTime, 0), args, xLabelOffset + graphMargin, labelColor); // X-axis
            DrawText(string.Format(CultureInfo.InvariantCulture, ValueFormat, Value[0]), args, graphStartPos + yLabelOffset + graphMargin, textColor); // Y-axis
            double total = normalizedData[0];
            iconPts?.Add(iconOffset + graphMargin);

            for (int i = 1; i < segments; i++)
            {
                var range = Math.Max(0, Math.Min(movingAverageRange / 2, Math.Min(i, segments - 1 - i)));
                int rangeLeft = i - range;
                int rangeRight = i + range;

                for (int j = previousRangeLeft; j < rangeLeft; j++)
                {
                    total -= normalizedData[j];
                }
                
                for (int j = previousRangeRight + 1; j <= rangeRight; j++)
                {
                    total += normalizedData[j];
                }

                previousRangeLeft = rangeLeft;
                previousRangeRight = rangeRight;

                var pos = new Vector2((float)(i * distanceMultiplier), (float)(canvas.ActualHeight * (1 - total / (range * 2 + 1)))) + graphMargin;
                if (i % step == 0 && Math.Abs(pos.X - canvas.ActualWidth) > 30)
                {
                    // All the pts are used for graph but label is skipped in between if not enough space.
                    var timeString = GetElapsedTimeString(StartTime, Interval * i);
                    var formattedValue = string.Format(CultureInfo.InvariantCulture, ValueFormat, Value[i]);
                    DrawText(timeString, args, new Vector2(pos.X, 0) + xLabelOffset, labelColor); // X-axis
                    DrawText(formattedValue, args, pos + yLabelOffset, textColor); // Y-axis
                    iconPts?.Add(new Vector2(pos.X, 0) + iconOffset);
                }
                cpb.AddLine(pos + new Vector2(13,0));
            }

            // Make the graph reach end of canvas.
            var graphLastRange = Math.Max(0, Math.Min(movingAverageRange / 2, Math.Min(segments, segments - 1 - segments)));
            var graphLastPos = new Vector2((float)(segments * distanceMultiplier * 1.2f), (float)(canvas.ActualHeight * (1 - total / (graphLastRange * 2 + 1)))) + graphMargin;
            var graphLastPosBottom = new Vector2((float)(segments * distanceMultiplier * 1.2f), (float)canvas.ActualHeight) + graphMargin;
            var graphLastPosLoopClose = new Vector2(0, (float)canvas.ActualHeight);
            cpb.AddLine(graphLastPos);
            cpb.AddLine(graphLastPosBottom);
            cpb.AddLine(graphLastPosLoopClose);
            cpb.EndFigure(CanvasFigureLoop.Open);

            args.DrawingSession.FillGeometry(CanvasGeometry.CreatePath(cpb), brush);

            // Horizontal line
            args.DrawingSession.DrawLine(new Vector2(0, (float)canvas.ActualHeight - 30f),
                new Vector2((float)canvas.ActualWidth, (float)canvas.ActualHeight - 30f),
                lineColor);

            DrawWeatherCodes(iconPts, step);
        }

        private void DrawWeatherCodes(List<Vector2> pts, int step)
        {
            if (!WeatherCodes.Any()|| pts is null)
            {
                Conditions = [];
                return;
            }

            Conditions = new HourlyConditions[pts.Count];
            for (int i = 0; i < pts.Count; i++)
            {
                var pt = pts[i];
                var weatherIndex = i * step;
                if (weatherIndex >= WeatherCodes.Length)
                    break;

                var hour = StartTime.AddHours(weatherIndex * Interval).Hour;
                Conditions[i] = new HourlyConditions(WeatherCodes[weatherIndex], hour >= 6 && hour < 18, pt.X, pt.Y);
            }
        }

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            //TODO: mouse over line draw
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) => UpdateTheme();

        private void UserControl_ActualThemeChanged(FrameworkElement sender, object args) => UpdateTheme();

        private void UpdateTheme()
        {
            if (this.ActualTheme == ElementTheme.Dark)
            {
                labelColor = Color.FromArgb(100, 255, 255, 255);
                lineColor = Color.FromArgb(50, 255, 255, 255);
                textColor = Colors.White;
            }
            else
            {
                labelColor = Color.FromArgb(100, 0, 0, 0);
                lineColor = Color.FromArgb(50, 0, 0, 0);
                textColor = Colors.Black;
            }
            canvas?.Invalidate();
        }

        private static void DrawText(string text, CanvasDrawEventArgs args, Vector2 pos, Color color)
        {
            var textLayout = new CanvasTextLayout(args.DrawingSession, text, new CanvasTextFormat() { FontSize = 12 }, 0.0f, 0.0f)
            {
                WordWrapping = CanvasWordWrapping.NoWrap
            };
            args.DrawingSession.DrawTextLayout(textLayout, pos, color);
        }

        private static float ConvertToRange(float oldStart, float oldEnd, float newStart, float newEnd, float value)
        {
            float scale = (newEnd - newStart) / (oldEnd - oldStart);
            return newStart + ((value - oldStart) * scale);
        }

        private static string GetElapsedTimeString(DateTime start, int elapsed)
        {
            return start.AddHours(elapsed).ToString("HH:mm");
        }
    }
}
