using Avalonia;
using Avalonia.Controls;
using Drizzle.Models.Shaders;
using System.Threading;
using System.Threading.Tasks;

namespace Drizzle.UI.Avalonia.UserControls.Shaders;

/// <summary>
/// DrawControl with shader switch transition effect.
/// </summary>
public partial class DrawControlEx : UserControl
{
    public ShaderModel? Shader
    {
        get => GetValue(ShaderProperty);
        set => SetValue(ShaderProperty, value);
    }

    public static readonly StyledProperty<ShaderModel?> ShaderProperty =
        AvaloniaProperty.Register<DrawControlEx, ShaderModel?>(nameof(Shader), null);

    public ShaderModel? Shader1
    {
        get => GetValue(Shader1Property);
        private set => SetValue(Shader1Property, value);
    }

    public static readonly StyledProperty<ShaderModel?> Shader1Property =
        AvaloniaProperty.Register<DrawControlEx, ShaderModel?>(nameof(Shader1), null);

    public ShaderModel? Shader2
    {
        get => GetValue(Shader2Property);
        private set => SetValue(Shader2Property, value);
    }

    public static readonly StyledProperty<ShaderModel?> Shader2Property =
        AvaloniaProperty.Register<DrawControlEx, ShaderModel?>(nameof(Shader2), null);

    public bool IsPaused
    {
        get => GetValue(IsPausedProperty);
        set => SetValue(IsPausedProperty, value);
    }

    public static readonly StyledProperty<bool> IsPausedProperty =
        AvaloniaProperty.Register<DrawControlEx, bool>(nameof(IsPaused), false);

    public bool IsPausedShader1
    {
        get => GetValue(IsPausedShader1Property);
        private set => SetValue(IsPausedShader1Property, value);
    }

    public static readonly StyledProperty<bool> IsPausedShader1Property =
        AvaloniaProperty.Register<DrawControlEx, bool>(nameof(IsPausedShader1), false);

    public bool IsPausedShader2
    {
        get => GetValue(IsPausedShader2Property);
        private set => SetValue(IsPausedShader2Property, value);
    }

    public static readonly StyledProperty<bool> IsPausedShader2Property =
        AvaloniaProperty.Register<DrawControlEx, bool>(nameof(IsPausedShader2), false);

    public bool IsVisibleShader1
    {
        get => GetValue(IsVisibleShader1Property);
        private set => SetValue(IsVisibleShader1Property, value);
    }

    public static readonly StyledProperty<bool> IsVisibleShader1Property =
        AvaloniaProperty.Register<DrawControlEx, bool>(nameof(IsVisibleShader1), true);

    public bool IsVisibleShader2
    {
        get => GetValue(IsVisibleShader2Property);
        private set => SetValue(IsVisibleShader2Property, value);
    }

    public static readonly StyledProperty<bool> IsVisibleShader2Property =
        AvaloniaProperty.Register<DrawControlEx, bool>(nameof(IsVisibleShader2), false);

    private readonly SemaphoreSlim transitionSemaphore = new(1, 1);

    public DrawControlEx()
    {
        InitializeComponent();
        ShaderProperty.Changed.AddClassHandler<DrawControlEx>(OnPropertyChanged);
        IsPausedProperty.Changed.AddClassHandler<DrawControlEx>(OnPropertyChanged);
    }

    private async void OnPropertyChanged(DrawControlEx sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ShaderProperty)
        {
            try
            {
                await transitionSemaphore.WaitAsync();

                var newShader = e.NewValue as ShaderModel;
                if (Shader1 is null)
                {
                    Shader1 = newShader;
                    IsPausedShader1 = false;

                    IsVisibleShader1 = true;
                    IsVisibleShader2 = false;

                    if (e.OldValue is not null)
                        await Task.Delay(100);
                    Shader2 = null;
                }
                else
                {
                    Shader2 = newShader;
                    IsPausedShader2 = false;

                    IsVisibleShader1 = false;
                    IsVisibleShader2 = true;

                    await Task.Delay(100);
                    Shader1 = null;
                }
            }
            finally
            {
                transitionSemaphore.Release();
            }
        }
        else if (e.Property == IsPausedProperty)
        {
            var isPaused = (bool)e.NewValue!;
            if (Shader1 is not null)
                IsPausedShader1 = isPaused;
            else if (Shader2 is not null)
                IsPausedShader2 = isPaused;
        }
    }
}