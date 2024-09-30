using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Drizzle.UI.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Drizzle.UI.Avalonia.Views;

public partial class HelpView : UserControl
{
    public HelpView()
    {
        InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<HelpViewModel>();
    }
}