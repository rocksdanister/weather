using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Drizzle.UI.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Drizzle.UI.Avalonia.Views;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<AboutViewModel>();
    }
}