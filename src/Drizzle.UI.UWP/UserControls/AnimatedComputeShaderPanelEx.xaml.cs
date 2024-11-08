using ComputeSharp.Uwp;
using Drizzle.Models.Enums;
using Drizzle.UI.Shared.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Drizzle.UI.UWP.UserControls;

/// <summary>
/// AnimatedComputeShaderPanel with shader switch transition effect.
/// </summary>
public sealed partial class AnimatedComputeShaderPanelEx : UserControl
{
    public ShaderViewModel Shader
    {
        get { return (ShaderViewModel)GetValue(ShaderProperty); }
        set { SetValue(ShaderProperty, value); }
    }

    public static readonly DependencyProperty ShaderProperty =
        DependencyProperty.Register("Shader", typeof(ShaderViewModel), typeof(AnimatedComputeShaderPanelEx), new PropertyMetadata(null, OnDependencyPropertyChanged));

    public ShaderQuality Quality
    {
        get { return (ShaderQuality)GetValue(QualityProperty); }
        set { SetValue(QualityProperty, value); }
    }

    public static readonly DependencyProperty QualityProperty =
        DependencyProperty.Register("Quality", typeof(ShaderQuality), typeof(AnimatedComputeShaderPanelEx), new PropertyMetadata(ShaderQuality.optimized, OnDependencyPropertyChanged));

    public IShaderRunner ShaderRunner1
    {
        get { return (IShaderRunner)GetValue(ShaderRunner1Property); }
        private set { SetValue(ShaderRunner1Property, value); }
    }

    public static readonly DependencyProperty ShaderRunner1Property =
        DependencyProperty.Register("ShaderRunner1", typeof(IShaderRunner), typeof(AnimatedComputeShaderPanelEx), new PropertyMetadata(null));

    public IShaderRunner ShaderRunner2
    {
        get { return (IShaderRunner)GetValue(ShaderRunner2Property); }
        private set { SetValue(ShaderRunner2Property, value); }
    }

    public static readonly DependencyProperty ShaderRunner2Property =
        DependencyProperty.Register("ShaderRunner2", typeof(IShaderRunner), typeof(AnimatedComputeShaderPanelEx), new PropertyMetadata(null));

    public bool IsPaused
    {
        get { return (bool)GetValue(IsPausedProperty); }
        set { SetValue(IsPausedProperty, value); }
    }

    public static readonly DependencyProperty IsPausedProperty =
        DependencyProperty.Register("IsPaused", typeof(bool), typeof(AnimatedComputeShaderPanelEx), new PropertyMetadata(false, OnDependencyPropertyChanged));

    public bool IsPausedShader1
    {
        get { return (bool)GetValue(IsPausedShader1Property); }
        private set { SetValue(IsPausedShader1Property, value); }
    }

    public static readonly DependencyProperty IsPausedShader1Property =
        DependencyProperty.Register("IsPausedShader1", typeof(bool), typeof(AnimatedComputeShaderPanelEx), new PropertyMetadata(false));

    public bool IsPausedShader2
    {
        get { return (bool)GetValue(IsPausedShader2Property); }
        private set { SetValue(IsPausedShader2Property, value); }
    }

    public static readonly DependencyProperty IsPausedShader2Property =
        DependencyProperty.Register("IsPausedShader2", typeof(bool), typeof(AnimatedComputeShaderPanelEx), new PropertyMetadata(false));

    public bool IsDynamicResolution
    {
        get { return (bool)GetValue(IsDynamicResolutionProperty); }
        set { SetValue(IsDynamicResolutionProperty, value); }
    }

    public static readonly DependencyProperty IsDynamicResolutionProperty =
        DependencyProperty.Register("IsDynamicResolution", typeof(bool), typeof(AnimatedComputeShaderPanelEx), new PropertyMetadata(false));

    public float ResolutionScaleShader1
    {
        get { return (float)GetValue(ResolutionScaleShader1Property); }
        private set { SetValue(ResolutionScaleShader1Property, value); }
    }

    public static readonly DependencyProperty ResolutionScaleShader1Property =
        DependencyProperty.Register("ResolutionScaleShader1", typeof(float), typeof(AnimatedComputeShaderPanelEx), new PropertyMetadata(1f));

    public float ResolutionScaleShader2
    {
        get { return (float)GetValue(ResolutionScaleShader2Property); }
        private set { SetValue(ResolutionScaleShader2Property, value); }
    }

    public static readonly DependencyProperty ResolutionScaleShader2Property =
        DependencyProperty.Register("ResolutionScaleShader2", typeof(float), typeof(AnimatedComputeShaderPanelEx), new PropertyMetadata(1f));

    private static void OnDependencyPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
    {
        var obj = s as AnimatedComputeShaderPanelEx;
        if (e.Property == ShaderProperty)
        {
            var shaderVm = e.NewValue as ShaderViewModel;
            if (obj.ShaderRunner1 is null)
            {
                obj.ShaderRunner1 = shaderVm?.Runner;
                obj.ShaderRunner2 = null;
            }
            else
            {
                obj.ShaderRunner2 = shaderVm?.Runner;
                obj.ShaderRunner1 = null;
            }
            obj.UpdateQuality();
        }
        else if (e.Property == QualityProperty)
        {
            obj.UpdateQuality();
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

    public AnimatedComputeShaderPanelEx()
    {
        this.InitializeComponent();
    }
}
