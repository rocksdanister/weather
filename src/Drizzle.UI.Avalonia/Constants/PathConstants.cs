using System;
using System.IO;

namespace Drizzle.UI.Avalonia.Constants;

public static class PathConstants
{
    public static readonly string CacheDir = GetCachePath();

    public static readonly string SettingsDir = GetSettingsPath();

    public static readonly string SettingsFile = Path.Combine(SettingsDir, "Settings.json");

    private static string GetCachePath()
    {
        if (OperatingSystem.IsLinux())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "Lively Weather", "Cache");
        else if (OperatingSystem.IsMacOS())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Caches", "Lively Weather");
        else if (OperatingSystem.IsWindows())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Lively Weather", "Cache");
        throw new NotImplementedException();
    }

    private static string GetSettingsPath()
    {
        if (OperatingSystem.IsLinux())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "Lively Weather");
        else if (OperatingSystem.IsMacOS())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Preferences", "Lively Weather");
        else if (OperatingSystem.IsWindows())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Lively Weather");
        else
            throw new NotImplementedException();
    }
}
