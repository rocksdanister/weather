using Avalonia;
using Avalonia.Controls;
using Drizzle.Common.Helpers;

namespace Drizzle.UI.Avalonia.UserControls.Cards;

public partial class Compass : UserControl
{
    public static readonly StyledProperty<float?> ValueProperty =
      AvaloniaProperty.Register<Compass, float?>(nameof(Value));

    // Speed
    public float? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<float?> DirectionProperty =
        AvaloniaProperty.Register<Compass, float?>(nameof(Direction));

    // Degree
    public float? Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    public static readonly StyledProperty<float> DirectionNormalizedProperty =
        AvaloniaProperty.Register<Compass, float>(nameof(DirectionNormalized));

    public float DirectionNormalized
    {
        get => GetValue(DirectionNormalizedProperty);
        private set => SetValue(DirectionNormalizedProperty, value);
    }

    public static readonly StyledProperty<string> UnitProperty =
        AvaloniaProperty.Register<Compass, string>(nameof(Unit), string.Empty);

    public string Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public static readonly StyledProperty<string> CardinalCoordinatesProperty =
        AvaloniaProperty.Register<Compass, string>(nameof(CardinalCoordinates), string.Empty);

    public string CardinalCoordinates
    {
        get => GetValue(CardinalCoordinatesProperty);
        private set => SetValue(CardinalCoordinatesProperty, value);
    }

    public Compass()
    {
        InitializeComponent();

        DirectionProperty.Changed.AddClassHandler<Compass>(OnPropertyChanged);
    }

    private static void OnPropertyChanged(Compass sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == DirectionProperty)
        {
            if (sender.Direction is null)
                return;

            var direction = (float)sender.Direction;
            sender.DirectionNormalized = MathUtil.NormalizeAngle(direction) / 360f * 100f;
            sender.CardinalCoordinates = MathUtil.DegreeToCardinalString(direction);
        }
    }
}