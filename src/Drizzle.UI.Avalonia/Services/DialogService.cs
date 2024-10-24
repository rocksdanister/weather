using Drizzle.Common.Services;
using Drizzle.UI.Avalonia.Helpers;
using Drizzle.UI.Avalonia.Views;
using Drizzle.UI.Shared.ViewModels;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Drizzle.UI.Avalonia.Services;

public class DialogService : IDialogService
{
    private readonly IResourceService resources;

    public DialogService(IResourceService resources)
    {
        this.resources = resources;
    }

    public async Task ShowHelpDialogAsync()
    {
        await new ContentDialog()
        {
            Title = resources.GetString("StringHelp"),
            CloseButtonText = resources.GetString("StringOk"),
            Content = new HelpView()
        }.ShowAsync();
    }

    public async Task ShowAboutDialogAsync()
    {
        var dialog = new ContentDialog()
        {
            Title = resources.GetString("StringAbout"),
            CloseButtonText = resources.GetString("StringOk"),
            Content = new AboutView()
        };
        dialog.Resources["ContentDialogMinWidth"] = 550d;
        await dialog.ShowAsync();
    }

    public async Task ShowSettingsDialogAsync()
    {
        var vm = App.Services.GetRequiredService<SettingsViewModel>();
        var dialog = new ContentDialog()
        {
            Title = resources.GetString("StringSettings"),
            CloseButtonText = resources.GetString("StringOk"),
            Content = new SettingsView(vm)
        };
        dialog.Resources["ContentDialogMinWidth"] = 650d;
        dialog.Closed += (s, e) =>
        {
            vm.OnClose();
        };
        await dialog.ShowAsync();
    }

    public async Task<string> ShowDepthCreationDialogAsync()
    {
        var vm = App.Services.GetRequiredService<DepthEstimateViewModel>();
        var depthDialog = new ContentDialog
        {
            Title = resources.GetString("StringDepthApprox"),
            Content = new DepthEstimateView(vm),
            PrimaryButtonText = resources.GetString("StringContinue"),
            SecondaryButtonText = resources.GetString("StringCancel"),
            DefaultButton = ContentDialogButton.Primary,
            SecondaryButtonCommand = vm.CancelCommand,
            PrimaryButtonCommand = vm.RunCommand,
            IsPrimaryButtonEnabled = vm.IsModelExists,
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
            depthDialog.IsPrimaryButtonEnabled = vm.RunCommand.CanExecute(null) && !vm.IsRunning;
        };
        vm.CancelCommand.CanExecuteChanged += (s, e) =>
        {
            depthDialog.IsSecondaryButtonEnabled = !vm.IsRunning;
        };

        await vm.AddImageCommand.ExecuteAsync(null);
        await depthDialog.ShowAsync();
        // Dispose resources
        vm.OnClose();
        return vm.DepthAssetDir;
    }
}
