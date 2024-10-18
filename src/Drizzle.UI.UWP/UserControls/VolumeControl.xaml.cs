using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Drizzle.UI.UWP.UserControls;

public sealed partial class VolumeControl : UserControl
{
    public int Volume
    {
        get { return (int)GetValue(VolumeProperty); }
        set 
        {
            SetValue(VolumeProperty, value);
            UpdateAnimation();
        }
    }

    public static readonly DependencyProperty VolumeProperty =
        DependencyProperty.Register("Volume", typeof(int), typeof(VolumeControl), new PropertyMetadata(0));

    public VolumeControl()
    {
        this.InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (Volume > 0)
        {
            if (!wave.IsPlaying)
                _ = wave.PlayAsync(0, 1, true);
        }
        else
        {
            wave.Stop();
        }
    }

    private void Wave_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        Volume = 0;
    }
}
