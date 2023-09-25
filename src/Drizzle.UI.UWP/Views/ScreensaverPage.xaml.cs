using Drizzle.Common.Services;
using Drizzle.Models;
using Drizzle.UI.UWP.Services;
using Drizzle.UI.UWP.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace Drizzle.UI.UWP.Views;

public sealed partial class ScreensaverPage : Page
{
    private readonly ScreensaverViewModel viewModel;
    private readonly DispatcherTimer dispatcherTimer = new();
    private readonly Stopwatch stopwatch = new();
    private readonly long timeout = 3000;
    private bool isFlyoutOpen = false;
    private readonly CoreCursor cursor;

    public ScreensaverPage()
    {
        this.InitializeComponent();
        this.viewModel = App.Services.GetRequiredService<ScreensaverViewModel>();
        this.DataContext = viewModel;

        // To restore if null
        cursor = CoreWindow.GetForCurrentThread().PointerCursor;
        // Autohide controls when fullscreen mode
        CoreWindow.GetForCurrentThread().PointerMoved += PointerEvents;
        CoreWindow.GetForCurrentThread().PointerPressed += PointerEvents;
        CoreWindow.GetForCurrentThread().CharacterReceived += KeyboardEvents;
        dispatcherTimer.Tick += DispatcherTimer_Tick;
        dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
        dispatcherTimer.Start();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        BackgroundGridShadow.Receivers.Add(BackgroundGrid);
    }

    private void DispatcherTimer_Tick(object sender, object e)
    {
        if (viewModel.AutoHideMenu || viewModel.IsFullScreen)
        {
            // Hide after timeout if no flyout is open
            if (stopwatch.ElapsedMilliseconds > timeout && !isFlyoutOpen)
            {
                inputGrid.Visibility = Visibility.Collapsed;
                CoreWindow.GetForCurrentThread().PointerCursor = null;
                stopwatch.Reset();
            }
        }
        else
        {
            if (stopwatch.IsRunning)
                stopwatch.Reset();
        }
    }

    private void PointerEvents(CoreWindow sender, PointerEventArgs args) => InputReceived();

    private void KeyboardEvents(CoreWindow sender, CharacterReceivedEventArgs args) => InputReceived();

    private void InputReceived()
    {
        if (viewModel.AutoHideMenu || viewModel.IsFullScreen)
            stopwatch.Start();

        if (inputGrid.Visibility != Visibility.Visible)
        {
            inputGrid.Visibility = Visibility.Visible;
            CoreWindow.GetForCurrentThread().PointerCursor ??= cursor;
        }
    }

    // Prevent flyout closing when dialog is opened from child flyout
    private void Flyout_Closing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
    {
        if (viewModel.IsBusy)
            args.Cancel = true;
    }

    // Hook up parent flyouts only to prevent hiding when user interacting fullscreen mode
    private void Flyout_Opened(object sender, object e) => isFlyoutOpen = true;

    // Hook up parent flyouts only to prevent hiding when user interacting fullscreen mode
    private void Flyout_Closed(object sender, object e) => isFlyoutOpen = false;

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        CoreWindow.GetForCurrentThread().PointerMoved -= PointerEvents;
        CoreWindow.GetForCurrentThread().PointerPressed -= PointerEvents;
        CoreWindow.GetForCurrentThread().CharacterReceived -= KeyboardEvents;
        dispatcherTimer.Tick -= DispatcherTimer_Tick;
        dispatcherTimer.Stop();
        stopwatch.Stop();

        CoreWindow.GetForCurrentThread().PointerCursor ??= cursor;
    }

    // ElementName binding not working?!
    private void GridViewDeleteFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var menu = sender as MenuFlyoutItem;
        var obj = menu?.DataContext as UserImageModel;
        if (obj is not null)
        {
            if (viewModel.IsRainPropertyVisible)
                viewModel.RainBackgroundDeleteCommand.Execute(obj);
            else if (viewModel.IsSnowPropertyVisible)
                viewModel.SnowBackgroundDeleteCommand.Execute(obj);
            else if (viewModel.IsDepthPropertyVisible)
                viewModel.DepthBackgroundDeleteCommand.Execute(obj);
            else if (viewModel.IsFogPropertyVisible)
                viewModel.FogBackgroundDeleteCommand.Execute(obj);
        }
    }

    //protected override void OnNavigatedTo(NavigationEventArgs e)
    //{

    //}
}
