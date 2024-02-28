using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using WindowsEx.UI.Core;
using System.Reflection;
using Windows.Graphics.Display;
using Size = Windows.Foundation.Size;
using System.Threading.Tasks;
using Drizzle.UI.UWP.ViewModels;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using Drizzle.Common.Services;
using System.Diagnostics;
using Drizzle.Common;
using Drizzle.Common.Constants;
using Windows.Foundation.Collections;
using System.Collections.ObjectModel;
using System.Collections;
using Drizzle.Weather;
using Drizzle.Models.Weather;
using Drizzle.UI.Shared.Shaders.Models;
using Microsoft.Extensions.Logging;
using System.Threading;
using Drizzle.UI.UWP.AnimatedVisuals;

namespace Drizzle.UI.UWP.Views
{
    public sealed partial class ShellPage : Page
    {
        // Mouse
        private bool isMouseDrag = false;
        private readonly float dragDelta = 10;
        private float2 dragStartPosition = float2.Zero;

        // DI
        private readonly ShellViewModel shellVm;
        private readonly INavigator navigator;
        private readonly ILogger<ShellPage> logger;
        private readonly IUserSettings userSettings;
        private readonly ISoundService soundService;

        // Timer 
        private readonly DispatcherTimer dispatcherTimer = new();
        private readonly Stopwatch deactivatedStopwatch = new();
        private readonly Stopwatch searchIdleStopwatch = new();
        private readonly long deactivatedTimeout = 3000;
        private readonly long searchIdleTimeout = 500;
        private bool isWindowDeactivated = false;

        public ShellPage()
        {
            this.InitializeComponent();
            this.shellVm = App.Services.GetRequiredService<ShellViewModel>();
            this.navigator = App.Services.GetRequiredService<INavigator>();
            this.logger = App.Services.GetRequiredService<ILogger<ShellPage>>();
            this.userSettings = App.Services.GetRequiredService<IUserSettings>();
            this.soundService = App.Services.GetRequiredService<ISoundService>();
            this.DataContext = shellVm;

            if (App.IsTenFoot)
            {
                // Ref: https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/turn-off-overscan
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            }

            //https://learn.microsoft.com/en-us/windows/apps/develop/title-bar?tabs=winui2#interactive-content
            // Hide default title bar.
            CoreApplicationViewTitleBar coreTitleBar =
                CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            // Set caption buttons background to transparent.
            ApplicationViewTitleBar titleBar =
                ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = this.ActualTheme == ElementTheme.Dark ? Colors.LightGray : Colors.Black;
            // Set XAML element as a drag region.
            Window.Current.SetTitleBar(AppTitleBar);
            // Register a handler for when the size of the overlaid caption control changes.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
            this.ActualThemeChanged += (s, e) =>
            {
                titleBar.ButtonForegroundColor = this.ActualTheme == ElementTheme.Dark ? Colors.LightGray : Colors.Black;
                AppTitleTextBlock.Foreground = new SolidColorBrush(this.ActualTheme == ElementTheme.Dark ? Colors.White : Colors.Black);
            };

            try
            {
                // Undocumented, ref: https://github.com/dillydylann/WindowsXamlHosting
                ((IInternalCoreWindow2)(object)CoreWindow.GetForCurrentThread()).WindowPositionChanged += MainPage_WindowPositionChanged;
            }
            catch (Exception ex)
            {
                logger.LogError($"WindowPositionChanged: {ex}");
            }
            CoreWindow.GetForCurrentThread().Activated += CoreWindow_Activated;

            this.Loaded += async (s, e) =>
            {
                // Trying to call once page is ready to avoid ComputeSharp/UWP Windowsize issue.
                await shellVm.RestoreWeather();
            };

            this.navigator.ContentPageChanged += (s, e) =>
            {
                if (e == ContentPageType.Screensaver)
                    navView.IsPaneOpen = false;
            };

            shellVm.DetectLocationCommand.CanExecuteChanged += (s, e) => 
            {
                // Could not get AnimatedIcon to work (asking for IAnimatedVisualSource2 ?) and wanted looping, so using player.
                if (!GeotagAnimation.IsPlaying)
                    _ = GeotagAnimation.PlayAsync(0, 1, true);
                else
                    GeotagAnimation.Stop();
            };

            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
        }

        // Animation playback
        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            // Titlebar
            if (args.WindowActivationState == CoreWindowActivationState.Deactivated)
                AppTitleTextBlock.Foreground = new SolidColorBrush(Colors.Gray);
            else
                AppTitleTextBlock.Foreground = new SolidColorBrush(this.ActualTheme == ElementTheme.Dark ? Colors.White : Colors.Black);

            switch (args.WindowActivationState)
            {
                case CoreWindowActivationState.Deactivated:
                    {
                        if (userSettings.Get<bool>(UserSettingsConstants.BackgroundPause))
                        {
                            shellVm.DepthProperty.Saturation =
                                shellVm.SnowProperty.Saturation =
                                shellVm.RainProperty.Saturation =
                                shellVm.FogProperty.Saturation =
                                shellVm.CloudsProperty.Saturation = 0.1f;

                            shellVm.SnowProperty.TimeMultiplier =
                                shellVm.CloudsProperty.TimeMultiplier =
                                shellVm.FogProperty.TimeMultiplier =
                                shellVm.RainProperty.TimeMultiplier = 0.1f;

                            isWindowDeactivated = true;
                            deactivatedStopwatch.Start();
                        }

                        if (userSettings.Get<bool>(UserSettingsConstants.BackgroundPauseAudio))
                            soundService.Pause();
                    }
                    break;
                case CoreWindowActivationState.CodeActivated:
                case CoreWindowActivationState.PointerActivated:
                    {
                        shellVm.DepthProperty.Saturation =
                            shellVm.SnowProperty.Saturation =
                            shellVm.RainProperty.Saturation =
                            shellVm.FogProperty.Saturation =
                            shellVm.CloudsProperty.Saturation = 1f;

                        shellVm.SnowProperty.TimeMultiplier =
                            shellVm.CloudsProperty.TimeMultiplier =
                            shellVm.RainProperty.TimeMultiplier =
                            shellVm.FogProperty.TimeMultiplier = 1f;

                        soundService.Play();
                        isWindowDeactivated = false;
                        deactivatedStopwatch.Reset();
                        shellVm.IsPausedShader1 = shellVm.IsPausedShader2 = false;
                    }
                    break;
            }
        }

        // Window movement
        private void MainPage_WindowPositionChanged(CoreWindow sender, object args)
        {
            // Skip when clicking or user preference
            if (sender.ActivationMode == CoreWindowActivationMode.Deactivated || userSettings.Get<bool>(UserSettingsConstants.ReducedMotion))
                return;

            //ref: https://stackoverflow.com/questions/31936154/get-screen-resolution-in-win10-uwp-app
            var displayInformation = DisplayInformation.GetForCurrentView();
            var screenSize = new Size(displayInformation.ScreenWidthInRawPixels,
                                      displayInformation.ScreenHeightInRawPixels);

            //var point = CoreWindow.GetForCurrentThread().PointerPosition;
            var point = new Windows.Foundation.Point(sender.Bounds.X, sender.Bounds.Y);
            var mouse = new float4
            {
                X = (float)(point.X / screenSize.Width),
                Y = (float)(point.Y / screenSize.Height),
                Z = 0,
                W = 0
            };

            // Skip animation when minimize-maximize..
            if (Math.Abs(shellVm.RainProperty.Mouse.X - mouse.X) > 1)
                return;

            shellVm.RainProperty.Mouse = mouse;
            shellVm.CloudsProperty.Mouse = mouse;
            shellVm.SnowProperty.Mouse = mouse;
            shellVm.FogProperty.Mouse = mouse;
            shellVm.DepthProperty.Mouse = mouse;
        }

        // Mouse drag
        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint((UIElement)sender);
            // Mouse pixel coords. xy: current (if MLB down), zw: click
            var mouse = new float4
            {
                X = (float)(point.Position.X / this.ActualWidth),
                Y = (float)(point.Position.Y / this.ActualHeight),
                Z = 0,
                W = 0
            };

            if (shellVm.IsMouseDrag
                && !(Math.Abs(point.Position.X - dragStartPosition.X) < dragDelta && Math.Abs(point.Position.Y - dragStartPosition.Y) < dragDelta)
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

        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _ = ((UIElement)sender).CapturePointer(e.Pointer);

            var point = e.GetCurrentPoint((UIElement)sender);
            dragStartPosition.X = (float)point.Position.X;
            dragStartPosition.Y = (float)point.Position.Y;
            isMouseDrag = true;

            // Close navView if open
            if (navView.IsPaneOpen)
                navView.IsPaneOpen = false;
        }

        private void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ((UIElement)sender).ReleasePointerCapture(e.Pointer);
            isMouseDrag = false;
        }

        // Navigation
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (navigator.Frame is null)
            {
                navigator.Frame = MainFrame;

                if (e.NavigationMode != NavigationMode.Back)
                {
                    navigator.NavigateTo(ContentPageType.Main);
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigator.Frame = null;
        }

        // Titlebar
        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            // Get the size of the caption controls and set padding.
            LeftPaddingColumn.Width = new GridLength(CoreApplication.GetCurrentView().TitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(CoreApplication.GetCurrentView().TitleBar.SystemOverlayRightInset);
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            AppTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TitleSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (!string.IsNullOrEmpty(sender.Text) && sender.Text.Length > 1)
                {
                    // To reduce API call search location after delay
                    searchIdleStopwatch.Reset();
                    searchIdleStopwatch.Start();
                }
                else
                {
                    // When sender.Text changes from >1 to empty or 1 length close the suggestionbox
                    shellVm.SearchSuggestions.Clear();
                }
            }
        }

        private void TitleSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            //sender.Text = string.Empty;
        }

        private async void TitleSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is not null)
            {
                // User selected an item from the suggestion list, take an action on it here.
                var selection = args.ChosenSuggestion as Location;
                if (selection is null)
                    return;

                // Change focus from suggestbox
                mainPage.Focus(FocusState.Programmatic);
                // Visual indication of selected location
                sender.Text = selection.DisplayName ?? string.Empty;

                // Update weather selection
                await shellVm.SetWeather(selection.DisplayName, selection.Latitude, selection.Longitude);
            }
            else
            {
                // Pick first if any
                var firstSelection = shellVm.SearchSuggestions?.FirstOrDefault();
                if (firstSelection is null)
                    return;

                // Change focus from suggestbox
                mainPage.Focus(FocusState.Programmatic);
                // Visual indication of selected location
                sender.Text = firstSelection.DisplayName ?? string.Empty;

                // Update weather selection
                await shellVm.SetWeather(firstSelection.DisplayName, firstSelection.Latitude, firstSelection.Longitude);
            }
            // Remove suggestbox input (TextChanged event is not UserInput)
            sender.Text = string.Empty;
            // Clear suggestions after selection
            shellVm.SearchSuggestions.Clear();
        }

        private async void DispatcherTimer_Tick(object sender, object e)
        {
            if (isWindowDeactivated && deactivatedStopwatch.ElapsedMilliseconds > deactivatedTimeout)
            {
                deactivatedStopwatch.Reset();
                shellVm.IsPausedShader1 = shellVm.IsPausedShader2 = true;
            }

            if (searchIdleStopwatch.ElapsedMilliseconds > searchIdleTimeout)
            {
                searchIdleStopwatch.Reset();
                await shellVm.FetchLocations(AppTitleSearch.Text);
            }
        }

        private void ErrorInfoBar_Closed(Microsoft.UI.Xaml.Controls.InfoBar sender, Microsoft.UI.Xaml.Controls.InfoBarClosedEventArgs args)
        {
            // Clear error message
            shellVm.ErrorMessage = null;
        }

        private void NavView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Don't sent it to the grid/page, navView is closed when clicking outside by using page.PointerPressed event.
            // Only handle when Pane open otherwise PointerPressed events for mousedrag effect won't work.
            if (navView.IsPaneOpen)
                e.Handled = true;
        }

        private void NavView_PaneOpening(Microsoft.UI.Xaml.Controls.NavigationView sender, object args)
        {
            navViewOverlay.Visibility = Visibility.Visible;
        }

        private void NavView_PaneClosing(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewPaneClosingEventArgs args)
        {
            navViewOverlay.Visibility = Visibility.Collapsed;
        }

        // ElementName binding not working?!
        private void DeleteLocationButton_Click(object sender, RoutedEventArgs e)
            => shellVm.DeleteLocationCommand.Execute((sender as Button)?.DataContext);
    }
}
