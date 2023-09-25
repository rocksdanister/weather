using Drizzle.UI.UWP.Helpers;
using System;
using System.Collections.Generic;
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

namespace Drizzle.UI.UWP.Views
{
    public sealed partial class HelpPage : Page
    {
        public HelpPage()
        {
            this.InitializeComponent();
        }

        private void WebsiteCard_Click(object sender, RoutedEventArgs e) => _ = LinkUtil.OpenBrowser("https://www.rocksdanister.com/weather");

        private void SoureCodeCard_Click(object sender, RoutedEventArgs e) => _ = LinkUtil.OpenBrowser("https://github.com/rocksdanister/weather");

        private void ContactCard_Click(object sender, RoutedEventArgs e) => _ = LinkUtil.OpenBrowser("https://github.com/rocksdanister/weather/wiki/Frequently-Asked-Questions-(FAQ)");
    }
}
