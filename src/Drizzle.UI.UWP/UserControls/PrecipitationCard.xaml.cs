using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Drizzle.UI.UWP.UserControls
{
    public sealed partial class PrecipitationCard : UserControl
    {
        public float? Value
        {
            get
            {
                return (float?)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(float?), typeof(PrecipitationCard), new PropertyMetadata(null));

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set
            {
                SetValue(UnitProperty, value);
            }
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(PrecipitationCard), new PropertyMetadata(string.Empty));

        public PrecipitationCard()
        {
            this.InitializeComponent();
        }
    }
}
