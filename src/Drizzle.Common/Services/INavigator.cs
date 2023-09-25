using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.Common.Services;

// https://github.com/jenius-apps/ambie/blob/main/src/AmbientSounds/Services/INavigator.cs
// MIT License Copyright(c) 2020 Jenius Apps

/// <summary>
/// Allows programmatic page navigation.
/// </summary>
public interface INavigator
{
    /// <summary>
    /// Raised when the content page was changed.
    /// </summary>
    event EventHandler<ContentPageType>? ContentPageChanged;

    /// <summary>
    /// The root frame of the app.
    /// </summary>
    object? RootFrame { get; set; }

    /// <summary>
    /// The inner frame that can navigate. This must be set before
    /// any method is called.
    /// </summary>
    object? Frame { get; set; }

    /// <summary>
    /// Returns the name of the current page, or returns
    /// empty string if no page is set.
    /// </summary>
    string GetContentPageName();

    /// <summary>
    /// Navigates to the screensaver.
    /// </summary>
    void ToScreensaver();

    /// <summary>
    /// Toggle and return fullscreen status.
    /// </summary>
    bool ToFullscreen(bool isFullscreen);

    /// <summary>
    /// Attempts to navigate back.
    /// </summary>
    /// <param name="sourcePage">Optional. If provided,
    /// then specific go back functionality may be applied.
    /// For example, if ScreensaverPage is provided, the 
    /// RootFrame is used for GoBack.</param>
    void GoBack(string? sourcePage = null);

    /// <summary>
    /// Navigates to the page corresponding to the given enum.
    /// </summary>
    /// <param name="contentPage">The page to navigate to.</param>
    /// <param name="navArgs">Arguments to be passed to the new page during navigation.</param>
    void NavigateTo(ContentPageType contentPage, object? navArgs = null);
}

public enum ContentPageType
{
    Main,
    Screensaver,
    //Settings,
}
