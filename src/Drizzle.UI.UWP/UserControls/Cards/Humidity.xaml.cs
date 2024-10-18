using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Drizzle.UI.UWP.UserControls.Cards;

public sealed partial class Humidity : UserControl
{
    public float? Value
    {
        get { return (float?)GetValue(ValueProperty); }
        set 
        { 
            SetValue(ValueProperty, value); 
        }
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(float?), typeof(Humidity), new PropertyMetadata(null));

    public Humidity()
    {
        this.InitializeComponent();
    }
}
