using Avalonia;
using Avalonia.Controls;

namespace Drizzle.UI.Avalonia.UserControls.Cards;

public partial class Pressure : UserControl
{
    public static readonly StyledProperty<float?> ValueProperty =
    AvaloniaProperty.Register<Pressure, float?>(nameof(Value));

    public float? Value
    {
        get { return GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    public static readonly StyledProperty<string> UnitProperty =
        AvaloniaProperty.Register<Pressure, string>(nameof(Unit), string.Empty);

    public string Unit
    {
        get { return GetValue(UnitProperty); }
        set { SetValue(UnitProperty, value); }
    }

    public Pressure()
    {
        InitializeComponent();
    }
}