using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Drizzle.Common.Constants;
using Drizzle.Common.Services;
using Drizzle.UI.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Drizzle.UI.Avalonia.Views;

public partial class MainWindow : Window
{
    private readonly ShellViewModel shellVm;
    private readonly IUserSettings userSettings;
    private readonly INavigator navigator;

    // Mouse
    private bool isMouseDrag = false;
    private readonly float dragDelta = 0.1f;
    private Vector2 dragStartPosition = Vector2.Zero;

    // Timer
    private CancellationTokenSource locationSearchCts = new();
    private readonly DispatcherTimer dispatcherTimer = new();
    private readonly Stopwatch deactivatedStopwatch = new();
    private readonly long deactivatedTimeout = 3000;
    private bool isWindowDeactivated = false;

    public MainWindow()
    {
        InitializeComponent();
        this.shellVm = App.Services.GetRequiredService<ShellViewModel>();
        this.userSettings = App.Services.GetRequiredService<IUserSettings>();
        this.navigator = App.Services.GetRequiredService<INavigator>();

        this.Loaded += MainWindow_Loaded;
        this.PositionChanged += MainWindow_PositionChanged;
        this.Activated += MainWindow_Activated;
        this.Deactivated += MainWindow_Deactivated;
        this.PointerPressed += MainWindow_PointerPressed;
        this.PointerReleased += MainWindow_PointerReleased;
        this.PointerMoved += MainWindow_PointerMoved;

        navigator.RootFrame = this;
        navigator.Frame = MainFrame;
        navigator.NavigateTo(ContentPageType.Main);

        // Operating system buttons on right-side.
        if (OperatingSystem.IsMacOS())
            AppTitle.IsVisible = false;

        dispatcherTimer.Tick += DispatcherTimer_Tick;
        dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        dispatcherTimer.Start();

#if DEBUG
        //this.AttachDevTools();
        //RendererDiagnostics.DebugOverlays = RendererDebugOverlays.Fps;
        //RendererDiagnostics.DebugOverlays = RendererDebugOverlays.Fps | RendererDebugOverlays.RenderTimeGraph | RendererDebugOverlays.LayoutTimeGraph;
#endif
    }

    private async void MainWindow_Loaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        // On some Linux distro extend not possible.
        if (!this.IsExtendedIntoWindowDecorations)
        {
            AppTitle.IsVisible = false;
            LocationPaneInnerButton.Margin = new Thickness(0, 5, 0, 0);
            Grid.SetRow(MainCommandBar, 0);
        }
        // We are waiting for Window to be ready before running animations to avoid shader related issues.
        await shellVm.RestoreWeather();
    }

    private void MainWindow_Deactivated(object? sender, System.EventArgs e)
    {
        if (!userSettings.Get<bool>(UserSettingsConstants.BackgroundPause))
            return;

        foreach (var item in shellVm.ShaderViewModels.Select(x => x.Model))
        {
            item.Saturation = 0.1f;
            item.TimeMultiplier = 0.1f;
        }

        isWindowDeactivated = true;
        deactivatedStopwatch.Start();
    }

    private void MainWindow_Activated(object? sender, System.EventArgs e)
    {
        foreach (var item in shellVm.ShaderViewModels.Select(x => x.Model))
        {
            item.Saturation = 1f;
            item.TimeMultiplier = 1f;
        }

        isWindowDeactivated = false;
        deactivatedStopwatch.Reset();
        shellVm.IsPausedShader1 = shellVm.IsPausedShader2 = false;
    }

    private void MainWindow_PositionChanged(object? sender, PixelPointEventArgs e)
    {
        var resolution = GetMonitorResolution(this);
        var mouse = new Vector4((float)(e.Point.X/resolution.Width), (float)(e.Point.Y/resolution.Height), 0, 0);

        // Skip animation when minimize-maximize.
        if (Math.Abs(shellVm.RainProperty.Mouse.X - mouse.X) > 1)
            return;

        foreach (var item in shellVm.ShaderViewModels.Select(x => x.Model))
            item.Mouse = new Vector4(mouse.X, mouse.Y, 0, 0);
    }

    private void MainWindow_PointerMoved(object? sender, global::Avalonia.Input.PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        var mouse = new Vector4((float)(position.X / this.Bounds.Width), (float)(position.Y / this.Bounds.Height), 0, 0);

        if (shellVm.IsMouseDrag
            && !(Math.Abs(mouse.X - dragStartPosition.X) < dragDelta && Math.Abs(mouse.Y - dragStartPosition.Y) < dragDelta)
            && isMouseDrag)
        {
            shellVm.RainProperty.Mouse = mouse;
            shellVm.CloudsProperty.Mouse = mouse;
            shellVm.SnowProperty.Mouse = mouse;
        }

        // User preference
        if (!shellVm.IsReducedMotion)
        {
            shellVm.DepthProperty.Mouse = mouse;
            shellVm.FogProperty.Mouse = mouse;
        }
    }

    private void MainWindow_PointerPressed(object? sender, global::Avalonia.Input.PointerPressedEventArgs e)
    {
        var point = e.GetPosition(this);
        dragStartPosition = new()
        {
            X = (float)(point.X / this.Bounds.Width),
            Y = (float)(point.Y / this.Bounds.Height)
        };
        isMouseDrag = true;
    }

    private void MainWindow_PointerReleased(object? sender, global::Avalonia.Input.PointerReleasedEventArgs e)
    {
        isMouseDrag = false;
    }

    // Issue: Keyboard navigation not possible. Ref: https://github.com/AvaloniaUI/Avalonia/issues/8721
    // Issue: Crash when clearing SearchSuggestions if IsDropDownOpen="true". Ref: https://github.com/AvaloniaUI/Avalonia/issues/6128
    private async void AutoCompleteBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SearchBox.SelectedItem is not Models.Weather.Location selection)
            return;

        // SearchBox.Text = selection.DisplayName; not working.
        // Instead, we override the ToString method of Drizzle.Models.Weather.Location.
        await shellVm.SetWeather(selection.DisplayName, selection.Latitude, selection.Longitude);

        // Reset Text and Selection.
        SearchBox.Text = string.Empty;
        shellVm.SearchSuggestions.Clear();
    }

    private void AutoCompleteBox_KeyUp(object? sender, global::Avalonia.Input.KeyEventArgs e)
    {
        // If user press Enter without selecting any location.
        if (e.Key == global::Avalonia.Input.Key.Enter && SearchBox.SelectedItem is not Models.Weather.Location)
        {
            var firstSelection = shellVm.SearchSuggestions?.FirstOrDefault();
            if (firstSelection != null)
                SearchBox.SelectedItem = firstSelection;
        }
    }

    public async Task<IEnumerable<object>> SearchAsync(string searchText, CancellationToken _)
    {
        if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 2)
        {
            shellVm.SearchSuggestions.Clear();
            return [];
        }

        // Cancel the previous fetch request.
        locationSearchCts?.Cancel();
        locationSearchCts?.Dispose();
        locationSearchCts = new CancellationTokenSource();

        // Reduce fetch request, wait till user idle.
        await Task.Delay(500, locationSearchCts.Token);

        if (!locationSearchCts.Token.IsCancellationRequested)
        {
            await shellVm.FetchLocations(searchText);
            return shellVm.SearchSuggestions;
        }
        return [];
    }

    private void DispatcherTimer_Tick(object? sender, EventArgs e)
    {
        if (isWindowDeactivated && deactivatedStopwatch.ElapsedMilliseconds > deactivatedTimeout)
        {
            deactivatedStopwatch.Reset();
            shellVm.IsPausedShader1 = shellVm.IsPausedShader2 = true;
        }
    }

    private void ErrorInfoBar_Closed(FluentAvalonia.UI.Controls.InfoBar sender, FluentAvalonia.UI.Controls.InfoBarClosedEventArgs args)
    {
        // Clear error message
        shellVm.ErrorMessage = null;
    }

    private void DeleteLocationButton_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        shellVm.DeleteLocationCommand.Execute((sender as Button)?.DataContext);
    }

    private void LocationPane_Inner_Button_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        CloseLocationPanel();
    }

    private void LocationOverlay_PointerPressed(object? sender, global::Avalonia.Input.PointerPressedEventArgs e)
    {
        CloseLocationPanel();
    }

    private void CloseLocationPanel()
    {
        LocationPaneButton.IsChecked = false;
    }

    private static Size GetMonitorResolution(Window window)
    {
        var screen = window?.Screens.ScreenFromVisual(window);
        return screen != null ? new Size(screen.Bounds.Width, screen.Bounds.Height) : new Size(0, 0);
    }
}
