using Drizzle.Common.Services;
using Drizzle.UI.UWP.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Drizzle.UI.UWP.Services;

public class Navigator : INavigator
{
    /// <inheritdoc/>
    public event EventHandler<ContentPageType>? ContentPageChanged;

    public object? RootFrame { get; set; }

    /// <inheritdoc/>
    public object? Frame { get; set; }

    public void GoBack(string sourcePage = null)
    {
        throw new NotImplementedException();
    }

    public string GetContentPageName()
    {
        if (Frame is Frame f)
        {
            return f.CurrentSourcePageType?.Name ?? string.Empty;
        }
        return string.Empty;
    }

    public void NavigateTo(ContentPageType contentPage, object navArgs = null)
    {
        Type pageType = contentPage switch
        {
            ContentPageType.Main => typeof(MainPage),
            ContentPageType.Screensaver => typeof(ScreensaverPage),
            _ => typeof(ShellPage)
        };

        if (Frame is Frame f)
        {
            f.Navigate(pageType, navArgs, new DrillInNavigationTransitionInfo());
            ContentPageChanged?.Invoke(this, contentPage);
        }
    }

    public void ToScreensaver()
    {
        if (Frame is Frame f && f.CurrentSourcePageType != typeof(ScreensaverPage))
        {
            f.Navigate(typeof(ScreensaverPage), null, new DrillInNavigationTransitionInfo());
            ContentPageChanged?.Invoke(this, ContentPageType.Screensaver);
        }
    }

    public bool ToFullscreen(bool isFullscreen)
    {
        var view = ApplicationView.GetForCurrentView();
        if (isFullscreen)
            return view.TryEnterFullScreenMode();
        else
            view.ExitFullScreenMode();
        return false;
    }
}
