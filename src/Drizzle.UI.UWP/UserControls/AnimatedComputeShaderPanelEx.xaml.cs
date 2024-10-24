using ComputeSharp.Uwp;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Drizzle.UI.UWP.UserControls;

/// <summary>
/// AnimatedComputeShaderPanel with shader switch transition effect.
/// </summary>
public sealed partial class AnimatedComputeShaderPanelEx : UserControl
{
    public IShaderRunner ShaderRunner
    {
        get { return (IShaderRunner)GetValue(ShaderRunnerProperty); }
        set { SetValue(ShaderRunnerProperty, value); }
    }

    public static readonly DependencyProperty ShaderRunnerProperty =
        DependencyProperty.Register("ShaderRunner", typeof(IShaderRunner), typeof(AnimatedComputeShaderPanelEx), new PropertyMetadata(null, OnDependencyPropertyChanged));

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

    public float ResolutionScale
    {
        get { return (float)GetValue(ResolutionScaleProperty); }
        set { SetValue(ResolutionScaleProperty, value); }
    }

    public static readonly DependencyProperty ResolutionScaleProperty =
        DependencyProperty.Register("ResolutionScale", typeof(float), typeof(AnimatedComputeShaderPanelEx), new PropertyMetadata(1f, OnDependencyPropertyChanged));

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
        if (e.Property == ShaderRunnerProperty)
        {
            var newRunner = e.NewValue as IShaderRunner;
            if (obj.ShaderRunner1 is null)
            {
                obj.ShaderRunner1 = newRunner;
                obj.IsPausedShader1 = false;
                obj.IsPausedShader2 = true;

                obj.ShaderRunner2 = null;
            }
            else
            {
                obj.ShaderRunner2 = newRunner;
                obj.IsPausedShader1 = true;
                obj.IsPausedShader2 = false;

                obj.ShaderRunner1 = null;
            }
        }
        else if (e.Property == ResolutionScaleProperty)
        {
            var newScale = (float)e.NewValue;
            if (obj.ShaderRunner1 is not null)
                obj.ResolutionScaleShader1 = newScale;
            else if (obj.ShaderRunner2 is not null)
                obj.ResolutionScaleShader2 = newScale;
        }
        else if (e.Property == IsPausedProperty)
        {
            var isPaused = (bool)e.NewValue;
            if (obj.ShaderRunner1 is not null)
                obj.IsPausedShader1 = isPaused;
            else if (obj.ShaderRunner2 is not null)
                obj.IsPausedShader2 = isPaused;
        }
    }

    public AnimatedComputeShaderPanelEx()
    {
        this.InitializeComponent();
    }
}
