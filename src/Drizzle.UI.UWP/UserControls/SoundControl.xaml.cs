using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Drizzle.UI.UWP.UserControls
{
    public sealed partial class SoundControl : UserControl
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
            DependencyProperty.Register("Volume", typeof(int), typeof(SoundControl), new PropertyMetadata(0));

        public SoundControl()
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
}
