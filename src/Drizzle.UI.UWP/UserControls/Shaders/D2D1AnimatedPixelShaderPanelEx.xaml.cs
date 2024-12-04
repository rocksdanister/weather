using Drizzle.Models.Enums;
using Drizzle.UI.Shared.ViewModels;
using Drizzle.UI.UWP.Shaders.D2D1.Runners;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Drizzle.UI.UWP.UserControls.Shaders;

/// <summary>
///  D2D1AnimatedPixelShaderPanel with shader switch transition effect and other enhancements.
/// </summary>
public sealed partial class D2D1AnimatedPixelShaderPanelEx : UserControl
{
    public ShaderViewModel Shader
    {
        get { return (ShaderViewModel)GetValue(ShaderProperty); }
        set { SetValue(ShaderProperty, value); }
    }

    public static readonly DependencyProperty ShaderProperty =
        DependencyProperty.Register(nameof(Shader), typeof(ShaderViewModel), typeof(D2D1AnimatedPixelShaderPanelEx), new PropertyMetadata(null, OnDependencyPropertyChanged));

    public ShaderQuality Quality
    {
        get { return (ShaderQuality)GetValue(QualityProperty); }
        set { SetValue(QualityProperty, value); }
    }

    public static readonly DependencyProperty QualityProperty =
        DependencyProperty.Register(nameof(Quality), typeof(ShaderViewModel), typeof(D2D1AnimatedPixelShaderPanelEx), new PropertyMetadata(ShaderQuality.optimized, OnDependencyPropertyChanged));

    public ID2D1ShaderRunner ShaderRunner1
    {
        get { return (ID2D1ShaderRunner)GetValue(ShaderRunner1Property); }
        private set { SetValue(ShaderRunner1Property, value); }
    }

    public static readonly DependencyProperty ShaderRunner1Property =
        DependencyProperty.Register(nameof(ShaderRunner1), typeof(ID2D1ShaderRunner), typeof(D2D1AnimatedPixelShaderPanelEx), new PropertyMetadata(null));

    public ID2D1ShaderRunner ShaderRunner2
    {
        get { return (ID2D1ShaderRunner)GetValue(ShaderRunner2Property); }
        private set { SetValue(ShaderRunner2Property, value); }
    }

    public static readonly DependencyProperty ShaderRunner2Property =
        DependencyProperty.Register(nameof(ShaderRunner2), typeof(ID2D1ShaderRunner), typeof(D2D1AnimatedPixelShaderPanelEx), new PropertyMetadata(null));

    public bool IsPaused
    {
        get { return (bool)GetValue(IsPausedProperty); }
        set { SetValue(IsPausedProperty, value); }
    }

    public static readonly DependencyProperty IsPausedProperty =
        DependencyProperty.Register(nameof(ShaderRunner2), typeof(bool), typeof(D2D1AnimatedPixelShaderPanelEx), new PropertyMetadata(false, OnDependencyPropertyChanged));

    public bool IsPausedShader1
    {
        get { return (bool)GetValue(IsPausedShader1Property); }
        private set { SetValue(IsPausedShader1Property, value); }
    }

    public static readonly DependencyProperty IsPausedShader1Property =
        DependencyProperty.Register(nameof(IsPausedShader1), typeof(bool), typeof(D2D1AnimatedPixelShaderPanelEx), new PropertyMetadata(false));

    public bool IsPausedShader2
    {
        get { return (bool)GetValue(IsPausedShader2Property); }
        private set { SetValue(IsPausedShader2Property, value); }
    }

    public static readonly DependencyProperty IsPausedShader2Property =
        DependencyProperty.Register(nameof(IsPausedShader2), typeof(bool), typeof(D2D1AnimatedPixelShaderPanelEx), new PropertyMetadata(false));

    public bool IsDynamicResolution
    {
        get { return (bool)GetValue(IsDynamicResolutionProperty); }
        set { SetValue(IsDynamicResolutionProperty, value); }
    }

    public static readonly DependencyProperty IsDynamicResolutionProperty =
        DependencyProperty.Register(nameof(IsDynamicResolution), typeof(bool), typeof(D2D1AnimatedPixelShaderPanelEx), new PropertyMetadata(false));

    public float ResolutionScaleShader1
    {
        get { return (float)GetValue(ResolutionScaleShader1Property); }
        private set { SetValue(ResolutionScaleShader1Property, value); }
    }

    public static readonly DependencyProperty ResolutionScaleShader1Property =
        DependencyProperty.Register(nameof(ResolutionScaleShader1), typeof(float), typeof(D2D1AnimatedPixelShaderPanelEx), new PropertyMetadata(1f));

    public float ResolutionScaleShader2
    {
        get { return (float)GetValue(ResolutionScaleShader2Property); }
        private set { SetValue(ResolutionScaleShader2Property, value); }
    }

    public static readonly DependencyProperty ResolutionScaleShader2Property =
        DependencyProperty.Register(nameof(ResolutionScaleShader2), typeof(float), typeof(D2D1AnimatedPixelShaderPanelEx), new PropertyMetadata(1f));

    public float TargetFrameRate
    {
        get { return (float)GetValue(TargetFrameRateProperty); }
        set { SetValue(TargetFrameRateProperty, value); }
    }

    public static readonly DependencyProperty TargetFrameRateProperty =
        DependencyProperty.Register(nameof(TargetFrameRate), typeof(float), typeof(D2D1AnimatedPixelShaderPanelEx), new PropertyMetadata(60f, OnDependencyPropertyChanged));

    public float CappedFrameRate
    {
        get { return (float)GetValue(CappedFrameRateProperty); }
        private set { SetValue(CappedFrameRateProperty, value); }
    }

    public static readonly DependencyProperty CappedFrameRateProperty =
        DependencyProperty.Register(nameof(CappedFrameRate), typeof(float), typeof(D2D1AnimatedPixelShaderPanelEx), new PropertyMetadata(60f));

    public bool IsPerformanceMetricVisible
    {
        get { return (bool)GetValue(IsPerformanceMetricVisibleProperty); }
        set { SetValue(IsPerformanceMetricVisibleProperty, value); }
    }

    public static readonly DependencyProperty IsPerformanceMetricVisibleProperty =
        DependencyProperty.Register(nameof(IsPerformanceMetricVisible), typeof(bool), typeof(D2D1AnimatedPixelShaderPanelEx), new PropertyMetadata(false));

    private static void OnDependencyPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
    {
        var obj = s as D2D1AnimatedPixelShaderPanelEx;
        if (e.Property == ShaderProperty)
        {
            var shaderVm = e.NewValue as ShaderViewModel;
            if (obj.ShaderRunner1 is null)
            {
                obj.ShaderRunner1 = shaderVm?.D2D1ShaderRunner;
                // Briefly unpause incase the already paused to render initial frame
                obj.IsPausedShader1 = false;
                // Restore pause state
                obj.IsPausedShader1 = obj.IsPaused;

                obj.ShaderRunner2 = null;
            }
            else
            {
                obj.ShaderRunner2 = shaderVm?.D2D1ShaderRunner;
                obj.IsPausedShader2 = false;
                obj.IsPausedShader2 = obj.IsPaused;

                obj.ShaderRunner1 = null;
            }

            // Update shader preferences
            obj.UpdateQuality();
            obj.UpdateFrameRate();
        }
        else if (e.Property == TargetFrameRateProperty)
        {
            obj.UpdateFrameRate();
        }
        else if (e.Property == QualityProperty)
        {
            obj.UpdateQuality();
            obj.UpdateFrameRate();
        }
        else if (e.Property == IsPausedProperty)
        {
            var isPaused = (bool)e.NewValue;
            obj.IsPausedShader1 = obj.IsPausedShader2 = isPaused;
        }
    }

    private void UpdateQuality()
    {
        if (Shader is null)
            return;

        switch (Quality)
        {
            case ShaderQuality.optimized:
            case ShaderQuality.dynamic:
                {
                    if (ShaderRunner1 is not null)
                        ResolutionScaleShader1 = Shader.Model.ScaleFactor;
                    else if (ShaderRunner2 is not null)
                        ResolutionScaleShader2 = Shader.Model.ScaleFactor;
                }
                break;
            case ShaderQuality.maximum:
                {
                    if (ShaderRunner1 is not null)
                        ResolutionScaleShader1 = Shader.Model.MaxScaleFactor;
                    else if (ShaderRunner2 is not null)
                        ResolutionScaleShader2 = Shader.Model.MaxScaleFactor;
                }
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void UpdateFrameRate()
    {
        if (Shader is null)
            return;

        CappedFrameRate = 
            Quality == ShaderQuality.maximum ? TargetFrameRate : Math.Clamp(TargetFrameRate, 0f, Shader.Model.MaxFrameRate);
    }

    public D2D1AnimatedPixelShaderPanelEx()
    {
        this.InitializeComponent();
    }
}
