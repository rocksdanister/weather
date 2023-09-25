using Drizzle.Common.Services;
using Drizzle.UI.UWP.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Drizzle.UI.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly ShellViewModel shellVm;

        public MainPage()
        {
            this.InitializeComponent();
            this.shellVm = App.Services.GetRequiredService<ShellViewModel>();
            this.DataContext = this.shellVm;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundGridShadow.Receivers.Add(BackgroundGrid);
        }

        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{

        //}

        //protected override void OnNavigatedFrom(NavigationEventArgs e)
        //{

        //}
    }
}
