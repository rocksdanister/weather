using Avalonia.Controls;
using Drizzle.Common.Services;
using Drizzle.UI.Avalonia.Views;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using System;

namespace Drizzle.UI.Avalonia.Services;

public class Navigator : INavigator
{
    public event EventHandler<ContentPageType>? ContentPageChanged;

    public object? RootFrame { get; set; }

    // In Avalonia this will be ContentControl/TransitioningContentControl
    public object? Frame { get; set; }

    public void GoBack(string? sourcePage = null)
    {
        throw new NotImplementedException();
    }

    public string GetContentPageName()
    {
        if (Frame is ContentControl control)
        {
            return control.Content?.GetType().Name ?? string.Empty;
        }
        return string.Empty;
    }

    public void NavigateTo(ContentPageType contentPage, object? navArgs = null)
    {
        Type type = contentPage switch
        {
            ContentPageType.Main => typeof(MainView),
            ContentPageType.Screensaver => typeof(ScreensaverView),
            _ => throw new NotImplementedException()
        };

        if (Frame is Frame control)
        {
            // Issue: DrillInNavigationTransitionInfo causes weekday-selector ListBox to be not fully populated.
            control.Navigate(type, navArgs);
            ContentPageChanged?.Invoke(this, contentPage);
        }
    }

    public void ToScreensaver()
    {
        if (Frame is Frame control && control.Content?.GetType() != typeof(ScreensaverView))
        {
            control.Navigate(typeof(ScreensaverView), null, new SlideNavigationTransitionInfo());
            ContentPageChanged?.Invoke(this, ContentPageType.Screensaver);
        }
    }

    public bool ToFullscreen(bool isFullscreen)
    {
        if (RootFrame is Window window)
        {
            window.WindowState = isFullscreen ? WindowState.FullScreen : WindowState.Normal;
            return isFullscreen;
        }
        return false;
    }
}
