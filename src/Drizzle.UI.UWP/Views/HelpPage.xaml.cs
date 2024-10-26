using Drizzle.UI.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace Drizzle.UI.UWP.Views
{
    public sealed partial class HelpPage : Page
    {
        private HelpViewModel viewModel;

        public HelpPage()
        {
            this.InitializeComponent();
            this.viewModel = App.Services.GetRequiredService<HelpViewModel>();
        }
    }
}
