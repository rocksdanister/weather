﻿using CommunityToolkit.Mvvm.ComponentModel;
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

        /// <summary>
        /// Pattern to show data points, =1 show all, =2 skip 1..
        /// </summary>
        public int Step
        {
            get { return (int)GetValue(StepProperty); }
            set
            {
                SetValue(StepProperty, value);
                canvas?.Invalidate();
            }
        }

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register("Step", typeof(int), typeof(DailyGraph), new PropertyMetadata(1, OnDependencyPropertyChanged));

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

        private static void OnDependencyPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var obj = s as DailyGraph;
            if (e.Property == StepProperty)
                obj.Step = (int)e.NewValue;
        }

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
            set
            {
                var stepValue = value?.Where((value, index) => (index % Step) == 0).ToArray();
                SetValue(WeatherCodesProperty, stepValue);

                Conditions = [];
                if (stepValue is not null)
                {
                    Conditions = new HourlyConditions[stepValue.Length];
                    for (int i = 0; i < stepValue.Length; i++)
                    {
                        var hour = StartTime.Hour + i * Step * Interval;
                        Conditions[i] = new HourlyConditions(stepValue[i], hour >= 6 && hour < 18, 0, 0);
                    }
                }
            }
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
                return;

            // Keeping track for icons
            var pts = new List<Vector2>();

            using var brush = new CanvasLinearGradientBrush(canvas, new CanvasGradientStop[] {
                new CanvasGradientStop { Position = 0.0f, Color = Gradient1 },
                new CanvasGradientStop { Position = 1.0f, Color = Colors.Transparent }
            });
            brush.StartPoint = new Vector2(0, 0);
            brush.EndPoint = new Vector2(0, (float)canvas.ActualHeight);

            int movingAverageRange = 4;
            int segments = Value.Count();
            var min = MinValue != null ? MinValue : Value.Min();
            var max = MaxValue != null ? MaxValue : Value.Max();
            var normalizedData = Value.Select(x => ConvertToRange((float)min, (float)max, 0.25f, 0.55f, x)).ToArray();

            using var cpb = new CanvasPathBuilder(args.DrawingSession);
            cpb.BeginFigure(new Vector2(0, (float)(canvas.ActualHeight * (1 - normalizedData[0]))));
            DrawText(GetElapsedTimeString(StartTime, 0), args, new Vector2(5f, (float)canvas.ActualHeight - 25f), labelColor); // X-axis
            DrawText(string.Format(CultureInfo.InvariantCulture, ValueFormat, Value[0]), args, new Vector2(5f, (float)(canvas.ActualHeight * (1 - normalizedData[0]))) + new Vector2(5f, -25f), textColor); // Y-axis
            double total = normalizedData[0];
            pts.Add(new Vector2(5f, (float)canvas.ActualHeight - 63f));

            int previousRangeLeft = 0;
            int previousRangeRight = 0;
            var distanceMultiplier = canvas.ActualWidth / segments;

            for (int i = 1; i < Value.Count(); i++)
            {
                var range = Math.Max(0, Math.Min(movingAverageRange / 2, Math.Min(i, Value.Count() - 1 - i)));
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

                var pos = new Vector2((float)(i * distanceMultiplier), (float)(canvas.ActualHeight * (1 - total / (range * 2 + 1))));
                // Label alternatingly
                if (i % Step == 0)
                {
                    DrawText(GetElapsedTimeString(StartTime, Interval * i), args, new Vector2(pos.X + 5f, (float)canvas.ActualHeight - 25f), labelColor); // X-axis
                    //DrawText($"{Value[i]:00.0}{Unit}", args, pos + new Vector2(0f, -25f), Colors.White); // Y-axis
                    DrawText(string.Format(CultureInfo.InvariantCulture, ValueFormat, Value[i]), args, pos + new Vector2(0f, -25f), textColor); // Y-axis
                    pts.Add(new Vector2(pos.X + 5f, (float)canvas.ActualHeight - 63f));
                }
                cpb.AddLine(pos);
            }

            var lastRange = Math.Max(0, Math.Min(movingAverageRange / 2, Math.Min(Value.Count(), Value.Count() - 1 - Value.Count())));
            var lastPos = new Vector2((float)(Value.Count() * distanceMultiplier), (float)(canvas.ActualHeight * (1 - total / (lastRange * 2 + 1))));
            cpb.AddLine(lastPos);
            cpb.AddLine(new Vector2((float)(Value.Count() * distanceMultiplier), (float)canvas.ActualHeight));
            cpb.AddLine(new Vector2(0, (float)canvas.ActualHeight));
            cpb.EndFigure(CanvasFigureLoop.Open);

            args.DrawingSession.FillGeometry(CanvasGeometry.CreatePath(cpb), brush);

            args.DrawingSession.DrawLine(new Vector2(0, (float)canvas.ActualHeight - 30f),
                new Vector2((float)canvas.ActualWidth, (float)canvas.ActualHeight - 30f),
                lineColor);

            for (int i = 0; i < Conditions?.Length; i++)
            {
                var pt = pts[i];
                var item = Conditions[i];
                item.Left = pt.X;
                item.Top = pt.Y;
            }

            //args.DrawingSession.DrawLine(
            //    new Vector2(0, (float)canvas.ActualHeight - 50),
            //    new Vector2((float)canvas.ActualWidth, (float)canvas.ActualHeight - 50),
            //    Color.FromArgb(5, 255, 255, 255),
            //    2.5f,
            //    new CanvasStrokeStyle() { DashStyle = CanvasDashStyle.Dot });

            //canvas.Invalidate();
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
