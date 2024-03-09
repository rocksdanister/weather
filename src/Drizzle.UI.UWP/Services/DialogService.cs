using Drizzle.Common.Constants;
using Drizzle.Common;
using Drizzle.Common.Services;
using Drizzle.UI.UWP.ViewModels;
using Drizzle.UI.UWP.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Diagnostics;
using Drizzle.UI.UWP.Extensions;
using Drizzle.UI.UWP.Helpers;
using Windows.Storage;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Media;

namespace Drizzle.UI.UWP.Services;

//Issue: https://github.com/microsoft/microsoft-ui-xaml/issues/2331
public class DialogService : IDialogService
{
    private readonly ResourceLoader resourceLoader;

    public DialogService()
    {
        if (Windows.UI.Core.CoreWindow.GetForCurrentThread() is not null)
            resourceLoader = ResourceLoader.GetForCurrentView();
    }

    public async Task ShowSettingsDialogAsync()
    {
        var vm = App.Services.GetRequiredService<SettingsViewModel>();
        var dialog = new ContentDialog()
        {
            Title = resourceLoader?.GetString("StringSettings"),
            Content = new SettingsPage(vm),
            CloseButtonText = resourceLoader?.GetString("StringOk"),
            Background = (AcrylicBrush)Application.Current.Resources["AcrylicInAppFillColorBaseBrush"]
        };
        //dialog.Resources["ContentDialogMaxWidth"] = 1200;
        dialog.Resources["ContentDialogMinWidth"] = 650;
        dialog.Closed += (s, e) =>
        {
            vm.OnClose();
        };
        await dialog.ShowAsync();
    }

    public async Task ShowAboutDialogAsync()
    {
        await new ContentDialog()
        {
            Title = resourceLoader?.GetString("StringAbout"),
            Content = new AboutPage(),
            CloseButtonText = resourceLoader?.GetString("StringOk"),
            Background = (AcrylicBrush)Application.Current.Resources["AcrylicInAppFillColorBaseBrush"]
        }.ShowAsync();
    }

    public async Task ShowHelpDialogAsync()
    {
        await new ContentDialog()
        {
            Title = resourceLoader?.GetString("StringHelp"),
            Content = new HelpPage(),
            CloseButtonText = resourceLoader?.GetString("StringOk"),
            Background = (AcrylicBrush)Application.Current.Resources["AcrylicInAppFillColorBaseBrush"]
        }.ShowAsync();
    }

    public async Task<string> ShowDepthCreationDialogAsync()
    {
        var vm = App.Services.GetRequiredService<DepthEstimateViewModel>();
        // Don't use Acrylic background since this operation can be heavy
        var depthDialog = new ContentDialog
        {
            Title = resourceLoader?.GetString("StringDepthApprox"),
            Content = new DepthEstimateView(vm),
            PrimaryButtonText = resourceLoader?.GetString("StringContinue"),
            SecondaryButtonText = resourceLoader?.GetString("StringCancel"),
            DefaultButton = ContentDialogButton.Primary,
            SecondaryButtonCommand = vm.CancelCommand,
            PrimaryButtonCommand = vm.RunCommand,
            IsPrimaryButtonEnabled = vm.IsModelExists,
            Background = (AcrylicBrush)Application.Current.Resources["AcrylicInAppFillColorBaseBrush"],
        };
        vm.OnRequestClose += (_, _) => depthDialog.Hide();
        depthDialog.Closing += (s, e) =>
        {
            if (e.Result == ContentDialogResult.Primary)
                e.Cancel = true;
        };
        //binding canExecute not working
        vm.RunCommand.CanExecuteChanged += (_, _) =>
        {
            depthDialog.IsPrimaryButtonEnabled = !vm.IsRunning;
        };
        vm.CancelCommand.CanExecuteChanged += (s, e) =>
        {
            depthDialog.IsSecondaryButtonEnabled = !vm.IsRunning;
        };

        var file = await FilePickerUtil.ShowImageDialogAsync();
        if (file is not null)
        {
            // File access permission for onnx library
            // TODO: Rewrite to stream
            var copyFile = await file.CopyAsync(ApplicationData.Current.TemporaryFolder, file.Name, NameCollisionOption.GenerateUniqueName);
            vm.SelectedImage = copyFile.Path;

            await depthDialog.ShowAsync();
            // Dispose resources
            await vm.OnClose();
        }
        return vm.DepthAssetDir;
    }
}
