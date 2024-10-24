using Avalonia.Controls;
using Drizzle.UI.Shared.ViewModels;

namespace Drizzle.UI.Avalonia.Views;

public partial class DepthEstimateView : UserControl
{
    private readonly DepthEstimateViewModel viewModel;

    public DepthEstimateView(DepthEstimateViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        this.DataContext = viewModel;
    }

    private void ErrorInfoBar_Closing(FluentAvalonia.UI.Controls.InfoBar sender, FluentAvalonia.UI.Controls.InfoBarClosingEventArgs args)
    {
        viewModel.ErrorText = null;
    }
}