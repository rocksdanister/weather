using Drizzle.UI.UWP.Helpers;
using Drizzle.UI.UWP.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Drizzle.UI.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        private readonly AboutViewModel aboutVm;

        public AboutPage()
        {
            this.InitializeComponent();
            this.aboutVm = App.Services.GetRequiredService<AboutViewModel>();
        }

        private void GithubButton_Click(object sender, RoutedEventArgs e) => _ = LinkUtil.OpenBrowser("https://github.com/rocksdanister");

        private void TwitterButton_Click(object sender, RoutedEventArgs e) => _ = LinkUtil.OpenBrowser("https://twitter.com/rocksdanister");

        private void RedditButton_Click(object sender, RoutedEventArgs e) => _ = LinkUtil.OpenBrowser("https://reddit.com/u/rocksdanister");

        private void YoutubeButton_Click(object sender, RoutedEventArgs e) => _ = LinkUtil.OpenBrowser("https://youtube.com/@rocksdanister");

        private void EmailButton_Click(object sender, RoutedEventArgs e) => _ = LinkUtil.OpenBrowser("mailto:awoo.git@gmail.com");

    }
}
