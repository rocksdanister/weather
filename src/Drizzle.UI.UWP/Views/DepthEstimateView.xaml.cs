using Drizzle.Common.Helpers;
using Drizzle.ImageProcessing;
using Drizzle.UI.UWP.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Drizzle.UI.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DepthEstimateView : Page
    {
        private readonly DepthEstimateViewModel viewModel;

        public DepthEstimateView(DepthEstimateViewModel viewModel)
        {
            this.InitializeComponent();
            this.viewModel = viewModel;
            this.viewModel.PropertyChanged += Vm_PropertyChanged; 
        }

        private async void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DepthEstimateViewModel.SelectedImage) && viewModel.SelectedImage is not null)
            {
                // Resizing with blur to avoid aliasing on images with details.
                var shaderTexturePath = FileUtil.NextAvailableFilename(viewModel.SelectedImage);
                await Task.Run(() => ImageUtil.GaussianBlur(viewModel.SelectedImage, shaderTexturePath, 1, 800));
                viewModel.SelectedShaderProperties.ImagePath = shaderTexturePath;

                // Workaround: ShaderPanel not running otherwise.
                shaderPanel.IsPaused = true;
                await Task.Delay(100);
                shaderPanel.IsPaused = false;

                // Trigger fade in animation.
                shaderPanel.Visibility = Visibility.Visible;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundGridShadow.Receivers.Add(BackgroundGrid);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Unreliable, firing on startup.
        }
    }
}
