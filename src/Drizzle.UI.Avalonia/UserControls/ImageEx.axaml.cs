using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Drizzle.UI.Avalonia.UserControls;

public partial class ImageEx : UserControl
{
    public string Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly StyledProperty<string> SourceProperty =
        AvaloniaProperty.Register<ImageEx, string>(nameof(Source), null);

    public string SourceA
    {
        get => GetValue(SourceAProperty);
        private set => SetValue(SourceAProperty, value);
    }

    public static readonly StyledProperty<string> SourceAProperty =
        AvaloniaProperty.Register<ImageEx, string>(nameof(SourceA), null);

    public string SourceB
    {
        get => GetValue(SourceBProperty);
        private set => SetValue(SourceBProperty, value);
    }

    public static readonly StyledProperty<string> SourceBProperty =
        AvaloniaProperty.Register<ImageEx, string>(nameof(SourceB), null);

    public bool IsVisibleSourceA
    {
        get => GetValue(IsVisibleSourceAProperty);
        private set => SetValue(IsVisibleSourceAProperty, value);
    }

    public static readonly StyledProperty<bool> IsVisibleSourceAProperty =
        AvaloniaProperty.Register<ImageEx, bool>(nameof(IsVisibleSourceA), false);

    public bool IsVisibleSourceB
    {
        get => GetValue(IsVisibleSourceBProperty);
        private set => SetValue(IsVisibleSourceBProperty, value);
    }

    public static readonly StyledProperty<bool> IsVisibleSourceBProperty =
        AvaloniaProperty.Register<ImageEx, bool>(nameof(IsVisibleSourceB), false);


    private bool toggleFlag;

    public ImageEx()
    {
        InitializeComponent();
        SourceProperty.Changed.AddClassHandler<ImageEx>(OnPropertyChanged);
    }

    private void OnPropertyChanged(ImageEx sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == SourceProperty)
        {
            if (!toggleFlag)
            {
                SourceA = Source;
                IsVisibleSourceA = true;
                IsVisibleSourceB = false;
            }
            else
            {
                SourceB = Source;
                IsVisibleSourceB = true;
                IsVisibleSourceA = false;
            }
            toggleFlag = !toggleFlag;
        }
    }
}