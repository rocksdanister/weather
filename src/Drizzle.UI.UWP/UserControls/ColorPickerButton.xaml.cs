using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Drizzle.UI.UWP.UserControls
{
    public sealed partial class ColorPickerButton : UserControl
    {
        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set 
            {
                colorPicker.Color = value;
                SetValue(SelectedColorProperty, value); 
            }
        }

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPickerButton), new PropertyMetadata(Colors.Pink));

        public ColorPickerButton()
        {
            this.InitializeComponent();
        }

        private void ColorPicker_ColorChanged(Microsoft.UI.Xaml.Controls.ColorPicker sender, Microsoft.UI.Xaml.Controls.ColorChangedEventArgs args)
        {
            if (!args.NewColor.Equals(SelectedColor))
                SelectedColor = args.NewColor;
        }
    }
}
