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
    public async Task ShowHelpDialogAsync()
    {
        await new ContentDialog()
        {
            Title = ResourceUtil.GetString("StringHelp"),
            CloseButtonText = ResourceUtil.GetString("StringOk"),
            Content = new HelpView()
        }.ShowAsync();
    }

    public async Task ShowAboutDialogAsync()
    {
        var dialog = new ContentDialog()
        {
            Title = ResourceUtil.GetString("StringAbout"),
            CloseButtonText = ResourceUtil.GetString("StringOk"),
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
            Title = ResourceUtil.GetString("StringSettings"),
            CloseButtonText = ResourceUtil.GetString("StringOk"),
            Content = new SettingsView(vm)
        };
        dialog.Resources["ContentDialogMinWidth"] = 650d;
        dialog.Closed += (s, e) =>
        {
            vm.OnClose();
        };
        await dialog.ShowAsync();
    }

    public Task<string> ShowDepthCreationDialogAsync()
    {
        throw new NotImplementedException();
    }
}
