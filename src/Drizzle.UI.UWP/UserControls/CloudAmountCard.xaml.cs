using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Drizzle.UI.UWP.UserControls
{
    public sealed partial class CloudAmountCard : UserControl
    {
        public float? Value
        {
            get { return (float?)GetValue(ValueProperty); }
            set 
            { 
                SetValue(ValueProperty, value);
                ShowClouds(value is not null ? (float)value : 25f);
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(float?), typeof(CloudAmountCard), new PropertyMetadata(null));

        public CloudAmountCard()
        {
            this.InitializeComponent();
        }

        private void ShowClouds(float value)
        {
            clouds100.Visibility = 
                clouds75.Visibility  = 
                clouds50.Visibility = 
                clouds25.Visibility = Visibility.Collapsed;

            if (value >= 90)
                clouds100.Visibility = Visibility.Visible;
            if (value >= 75)
                clouds75.Visibility = Visibility.Visible;
            else if (value >= 50)
                clouds50.Visibility = Visibility.Visible;
            else
                clouds25.Visibility = Visibility.Visible;
        }
    }
}
