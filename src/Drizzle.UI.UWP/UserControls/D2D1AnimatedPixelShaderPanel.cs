using Drizzle.UI.UWP.Shaders.D2D1.Runners;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace Drizzle.UI.UWP.UserControls;

[TemplatePart(Name = "PART_CanvasAnimatedControl", Type = typeof(CanvasAnimatedControl))]
public sealed class D2D1AnimatedPixelShaderPanel : Control
{
    /// <summary>
    /// The wrapped <see cref="CanvasAnimatedControl"/> instance used to render frames.
    /// </summary>
    private CanvasAnimatedControl? canvasAnimatedControl;

    /// <summary>
    /// The <see cref="ID2D1ShaderRunner"/> instance currently in use, if any.
    /// </summary>
    private volatile ID2D1ShaderRunner? shaderRunner;

    private double resolutionScale = 1f;
    private bool isPerformanceMetricVisible = false;
    private readonly Stopwatch performanceMetricStopWatch = new();
    private int frameCount = 0;
    private float fps = 0;

    /// <summary>   
    /// Creates a new <see cref="D2D1AnimatedPixelShaderPanel"/> instance.
    /// </summary>
    public D2D1AnimatedPixelShaderPanel()
    {
        DefaultStyleKey = typeof(D2D1AnimatedPixelShaderPanel);

        Loaded += D2D1AnimatedPixelShaderPanel_Loaded;
        Unloaded += D2D1AnimatedPixelShaderPanel_Unloaded;
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        this.canvasAnimatedControl = (CanvasAnimatedControl)GetTemplateChild("PART_CanvasAnimatedControl")!;
        this.canvasAnimatedControl.Draw += CanvasAnimatedControl_Draw;

        //this.canvasAnimatedControl.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 24);
    }

    // Ensure the panel is not paused when the control is loaded
    private void D2D1AnimatedPixelShaderPanel_Loaded(object sender, RoutedEventArgs e)
    {
        if (this.canvasAnimatedControl is { } canvasAnimatedControl &&
            ShaderRunner is not null &&
            !IsPaused)
        {
            canvasAnimatedControl.Paused = false;
        }
    }

    // Always pause rendering when the control is unloaded
    private void D2D1AnimatedPixelShaderPanel_Unloaded(object sender, RoutedEventArgs e)
    {
        if (this.canvasAnimatedControl is { } canvasAnimatedControl)
        {
            canvasAnimatedControl.Paused = true;
        }
    }

    /// <summary>
    /// Draws a new frame on the wrapped <see cref="CanvasAnimatedControl"/> instance.
    /// </summary>
    /// <inheritdoc cref="ID2D1ShaderRunner.Execute"/>
    private void CanvasAnimatedControl_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
    {
        this.shaderRunner?.Execute(sender, args, resolutionScale);

        if (isPerformanceMetricVisible)
        {
            frameCount++;
            // Update every second
            if (performanceMetricStopWatch.ElapsedMilliseconds >= 1000)
            {
                fps = frameCount / (performanceMetricStopWatch.ElapsedMilliseconds / 1000f);
                frameCount = 0;
                performanceMetricStopWatch.Restart();
            }
            args.DrawingSession.DrawText($"FPS: {fps:F2}, Scale: {resolutionScale}", 10, (float)sender.Size.Height - 25f, Windows.UI.Colors.White);
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="ID2D1ShaderRunner"/> instance to use to render content.
    /// </summary>
    public ID2D1ShaderRunner? ShaderRunner
    {
        get => (ID2D1ShaderRunner?)GetValue(ShaderRunnerProperty);
        set => SetValue(ShaderRunnerProperty, value);
    }

    /// <summary>
    /// The <see cref="DependencyProperty"/> backing <see cref="ShaderRunner"/>.
    /// </summary>
    public static readonly DependencyProperty ShaderRunnerProperty = 
        DependencyProperty.Register(nameof(ShaderRunner), typeof(ID2D1ShaderRunner), typeof(D2D1AnimatedPixelShaderPanel), new PropertyMetadata(null, OnShaderRunnerPropertyChanged));

    /// <inheritdoc cref="DependencyPropertyChangedCallback"/>
    private static void OnShaderRunnerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        D2D1AnimatedPixelShaderPanel @this = (D2D1AnimatedPixelShaderPanel)d;
        ID2D1ShaderRunner? shaderRunner = (ID2D1ShaderRunner?)e.NewValue;

        // Save the new shader runner for later (as the dependency property cannot be accessed by the render thread)
        @this.shaderRunner = shaderRunner;

        // Pause or start the render thread if a runner is available
        if (@this.canvasAnimatedControl is { } canvasAnimatedControl)
        {
            bool shouldRender = @this.IsLoaded && !@this.IsPaused && shaderRunner is not null;

            canvasAnimatedControl.Paused = !shouldRender;
        }
    }

    /// <summary>
    /// Gets or sets whether or not the rendering is paused.
    /// </summary>
    public bool IsPaused
    {
        get => (bool)GetValue(IsPausedProperty);
        set => SetValue(IsPausedProperty, value);
    }

    /// <summary>
    /// The <see cref="DependencyProperty"/> backing <see cref="IsPaused"/>.
    /// </summary>
    public static readonly DependencyProperty IsPausedProperty = 
        DependencyProperty.Register(nameof(IsPaused), typeof(bool), typeof(D2D1AnimatedPixelShaderPanel), new PropertyMetadata(false, OnIsPausedPropertyChanged));

    /// <inheritdoc cref="DependencyPropertyChangedCallback"/>
    private static void OnIsPausedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        D2D1AnimatedPixelShaderPanel @this = (D2D1AnimatedPixelShaderPanel)d;
        bool isPaused = (bool)e.NewValue;

        if (@this.canvasAnimatedControl is { } canvasAnimatedControl)
        {
            bool shouldRender = @this.IsLoaded && !isPaused && @this.ShaderRunner is not null;

            canvasAnimatedControl.Paused = !shouldRender;
        }
    }

    public double ResolutionScale
    {
        get { return (double)GetValue(ResolutionScaleProperty); }
        set { SetValue(ResolutionScaleProperty, value); }
    }

    public static readonly DependencyProperty ResolutionScaleProperty =
        DependencyProperty.Register(nameof(ResolutionScale), typeof(double), typeof(D2D1AnimatedPixelShaderPanel), new PropertyMetadata(1f, OnResolutionScalePropertyChanged));

    private static void OnResolutionScalePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        D2D1AnimatedPixelShaderPanel @this = (D2D1AnimatedPixelShaderPanel)d;
        @this.resolutionScale = (double)e.NewValue;
    }

    public bool IsPerformanceMetricVisible
    {
        get { return (bool)GetValue(IsPerformanceMetricVisibleProperty); }
        set { SetValue(IsPerformanceMetricVisibleProperty, value); }
    }

    public static readonly DependencyProperty IsPerformanceMetricVisibleProperty =
        DependencyProperty.Register(nameof(IsPerformanceMetricVisible), typeof(bool), typeof(D2D1AnimatedPixelShaderPanel), new PropertyMetadata(false, OnIsPerformanceMetricVisiblePropertyChanged));

    private static void OnIsPerformanceMetricVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        D2D1AnimatedPixelShaderPanel @this = (D2D1AnimatedPixelShaderPanel)d;
        bool isPerformanceMetricVisible = (bool)e.NewValue;
        @this.isPerformanceMetricVisible = isPerformanceMetricVisible;
        if (isPerformanceMetricVisible)
            @this.performanceMetricStopWatch.Restart();
        else
            @this.performanceMetricStopWatch.Stop();
    }
}
