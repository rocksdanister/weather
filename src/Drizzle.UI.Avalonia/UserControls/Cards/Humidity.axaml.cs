using Avalonia;
using Avalonia.Controls;

namespace Drizzle.UI.Avalonia.UserControls.Cards;

public partial class Humidity : UserControl
{
    public static readonly StyledProperty<float?> ValueProperty =
    AvaloniaProperty.Register<Humidity, float?>(nameof(Value), null);

    public float? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Humidity()
    {
        InitializeComponent();
    }
}