﻿using Drizzle.Common.Helpers;
using Drizzle.ImageProcessing;
using Drizzle.UI.Shared.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
                // Fix: Shader control not running, possibly dialog related issue?
                await Task.Delay(100);
                this.FindName("ShaderPanel");
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
