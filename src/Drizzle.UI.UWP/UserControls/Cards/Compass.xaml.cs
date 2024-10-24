using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Drizzle.UI.UWP.UserControls.Cards;

public sealed partial class Compass : UserControl
{
    public float? Value
    {
        get { return (float?)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(float?), typeof(Compass), new PropertyMetadata(null));

    public float? Direction
    {
        get { return (float?)GetValue(DirectionProperty); }
        set
        {
            SetValue(DirectionProperty, value);
            DirectionNormalized = Direction ?? 0;
        }
    }

    public static readonly DependencyProperty DirectionProperty =
        DependencyProperty.Register("Direction", typeof(float?), typeof(Compass), new PropertyMetadata(null));

    public float DirectionNormalized
    {
        get { return (float)GetValue(DirectionNormalizedProperty); }
        private set 
        { 
            value = value < 0 ? value + 360 : value;
            value = value / 360f * 100f;
            SetValue(DirectionNormalizedProperty, value); 
        }
    }

    public static readonly DependencyProperty DirectionNormalizedProperty =
        DependencyProperty.Register("DirectionNormalized", typeof(float), typeof(Compass), new PropertyMetadata(null));

    public string Unit
    {
        get { return (string)GetValue(UnitProperty); }
        set { SetValue(UnitProperty, value); }
    }

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register("Unit", typeof(string), typeof(Compass), new PropertyMetadata(string.Empty));

    public Compass()
    {
        this.InitializeComponent();
    }
}
