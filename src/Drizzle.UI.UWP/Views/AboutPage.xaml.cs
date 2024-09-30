using Drizzle.UI.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace Drizzle.UI.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        private readonly AboutViewModel viewModel;

        public AboutPage()
        {
            this.InitializeComponent();
            this.viewModel = App.Services.GetRequiredService<AboutViewModel>();
        }
    }
}
