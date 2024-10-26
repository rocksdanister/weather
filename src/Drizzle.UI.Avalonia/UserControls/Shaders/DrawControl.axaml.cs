using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Drizzle.Models.Shaders;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;

namespace Drizzle.UI.Avalonia.UserControls.Shaders;

/// <summary>
/// A SKSL shader renderer that takes <see cref="ShaderModel"/> as input.<para>
/// Default uniforms:</para>
/// <list>
/// <item><c>u_Resolution</c>: The resolution of the output in pixels (width, height, depth).</item>
/// <item><c>u_Time</c>: A float representing the elapsed time since the shader started, used for animations.</item>
/// <item><c>u_Mouse</c>: A vector representing the mouse position (x, y, z, w), where x and y correspond to the screen coordinates.</item>
/// <item><c>u_Brightness</c>: A float (0 - 1) controlling the brightness of the shader output.</item>
/// <item><c>u_Saturation</c>: A float (0 - 1) controlling the saturation level of the colors in the shader output.</item>
/// </list>
/// Based on: <see href="https://github.com/wieslawsoltes/EffectsDemo"/>
/// </summary>
public partial class DrawControl : UserControl
{
    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<DrawControl, Stretch>(nameof(Stretch), Stretch.UniformToFill);

    public Stretch Stretch
    {
        get { return GetValue(StretchProperty); }
        set { SetValue(StretchProperty, value); }
    }

    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
    AvaloniaProperty.Register<DrawControl, StretchDirection>(
        nameof(StretchDirection),
        StretchDirection.Both);

    public StretchDirection StretchDirection
    {
        get { return GetValue(StretchDirectionProperty); }
        set { SetValue(StretchDirectionProperty, value); }
    }

    public static readonly StyledProperty<ShaderModel?> ShaderProperty =
        AvaloniaProperty.Register<DrawControl, ShaderModel?>(nameof(Shader), null);

    public ShaderModel? Shader
    {
        get { return GetValue(ShaderProperty); }
        set { SetValue(ShaderProperty, value); }
    }

    public static readonly StyledProperty<bool> IsPausedProperty =
        AvaloniaProperty.Register<DrawControl, bool>(nameof(IsPaused), false);

    public bool IsPaused
    {
        get { return GetValue(IsPausedProperty); }
        set { SetValue(IsPausedProperty, value); }
    }

    private CompositionCustomVisual? customVisual;
    private DrawCompositionCustomVisualHandler? compositionHandler;

    public DrawControl()
    {
        InitializeComponent();
        ShaderProperty.Changed.AddClassHandler<DrawControl>(OnPropertyChanged);
        IsPausedProperty.Changed.AddClassHandler<DrawControl>(OnPropertyChanged);
        OpacityProperty.Changed.AddClassHandler<DrawControl>(OnPropertyChanged);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var elemVisual = ElementComposition.GetElementVisual(this);
        var compositor = elemVisual?.Compositor;
        if (compositor is null)
        {
            return;
        }

        compositionHandler = new DrawCompositionCustomVisualHandler();
        customVisual = compositor.CreateCustomVisual(compositionHandler);
        ElementComposition.SetElementChildVisual(this, customVisual);

        LayoutUpdated += OnLayoutUpdated;

        customVisual.Size = new Vector2((float)Bounds.Size.Width, (float)Bounds.Size.Height);
        customVisual.SendHandlerMessage(
            new DrawPayload(
                HandlerCommand.Update,
                null, 
                Bounds.Size,
                Stretch, 
                StretchDirection));

        Start(Shader);
    }

    private void OnPropertyChanged(DrawControl sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ShaderProperty)
        {
            Stop();
            Start(Shader);
        }
        else if (e.Property == IsPausedProperty)
        {
            if (IsPaused)
                Pause();
            else
                Resume();
        }
        else if (e.Property == OpacityProperty)
        {
            IsVisible = Opacity != 0;
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        LayoutUpdated -= OnLayoutUpdated;

        Stop();
        DisposeImpl();
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (customVisual == null)
            return;

        customVisual.Size = new Vector2((float)Bounds.Size.Width, (float)Bounds.Size.Height);
        customVisual.SendHandlerMessage(
            new DrawPayload(
                HandlerCommand.Update, 
                null, 
                Bounds.Size,
                Stretch, 
                StretchDirection));
    }

    private void Start(ShaderModel? model)
    {
        if (model is null)
        {
            DisposeImpl(); 
            return;
        }

        customVisual?.SendHandlerMessage(
            new DrawPayload(
                HandlerCommand.Start,
                model,
                Bounds.Size,
                Stretch, 
                StretchDirection));
    }

    private void Stop()
    {
        customVisual?.SendHandlerMessage(new DrawPayload(HandlerCommand.Stop));
    }

    private void Pause()
    {
        customVisual?.SendHandlerMessage(new DrawPayload(HandlerCommand.Pause));
    }

    private void Resume()
    {
        customVisual?.SendHandlerMessage(new DrawPayload(HandlerCommand.Start));
    }

    private void DisposeImpl()
    {
        customVisual?.SendHandlerMessage(new DrawPayload(HandlerCommand.Dispose));
    }
}
