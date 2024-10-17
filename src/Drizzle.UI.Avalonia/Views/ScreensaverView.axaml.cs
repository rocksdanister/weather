using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Drizzle.Models;
using Drizzle.UI.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Drizzle.UI.Avalonia.Views;

public partial class ScreensaverView : UserControl
{
    private readonly ScreensaverViewModel viewModel;
    public ScreensaverView()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<ScreensaverViewModel>();
        this.DataContext = viewModel;
    }

    private void DeleteBackgroundMenuItem_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        var menu = sender as MenuItem;
        var obj = menu?.DataContext as UserImageModel;
        if (obj is not null)
            viewModel.DeleteBackgroundCommand.Execute(obj);
    }
}