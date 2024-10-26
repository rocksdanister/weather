using Avalonia;
using Avalonia.Controls;

namespace Drizzle.UI.Avalonia.UserControls.Cards;

public partial class Clouds : UserControl
{
    public static readonly StyledProperty<float?> ValueProperty =
        AvaloniaProperty.Register<Clouds, float?>(nameof(Value));

    public float? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<string> AnimatedVisualProperty =
    AvaloniaProperty.Register<Compass, string>(nameof(AnimatedVisual), string.Empty);

    public string AnimatedVisual
    {
        get => GetValue(AnimatedVisualProperty);
        private set => SetValue(AnimatedVisualProperty, value);
    }

    public Clouds()
    {
        InitializeComponent();

        ValueProperty.Changed.AddClassHandler<Clouds>(OnPropertyChanged);
    }

    private static void OnPropertyChanged(Clouds sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ValueProperty)
        {
            if (sender.Value is null)
                return;

            var value = (float)sender.Value;
            if (value >= 75)
                sender.AnimatedVisual = "/Assets/AnimatedIcons/Clouds75.json";
            else if (value >= 50)
                sender.AnimatedVisual = "/Assets/AnimatedIcons/Clouds50.json";
            else if (value >= 25)
                sender.AnimatedVisual = "/Assets/AnimatedIcons/Clouds25.json";
            else
                sender.AnimatedVisual = "/Assets/AnimatedIcons/Clouds3.json";
        }
    }
}