using Drizzle.Common.Services;
using System;
using Windows.ApplicationModel.Resources;
using Windows.Globalization;
using Windows.System.UserProfile;

namespace Drizzle.UI.UWP.Services;

public class ResourceService : IResourceService
{
    private readonly ResourceLoader resourceLoader;

    public ResourceService()
    {
        if (Windows.UI.Core.CoreWindow.GetForCurrentThread() is not null)
            resourceLoader = ResourceLoader.GetForCurrentView();
    }

    public string GetString(string resource)
    {
        return resourceLoader?.GetString(resource);
    }

    public void SetCulture(string name)
    {
        // Setting is persisted between sessions.
        // Ref: https://learn.microsoft.com/en-us/uwp/api/windows.globalization.applicationlanguages.primarylanguageoverride?view=winrt-26100
        if (string.IsNullOrEmpty(name) || string.Equals(name, ApplicationLanguages.PrimaryLanguageOverride, StringComparison.OrdinalIgnoreCase))
            return;

        ApplicationLanguages.PrimaryLanguageOverride = name;
    }

    public void SetSystemDefaultCulture()
    {
        SetCulture(GlobalizationPreferences.Languages[0]);
    }
}
