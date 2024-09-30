using System.Globalization;

namespace Drizzle.UI.Avalonia.Helpers;

public static class ResourceUtil
{
    public static void SetCulture(string name)
    {
        var culture = new CultureInfo(name);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        // ResourceManager.GetString ignores this and uses CurrentUICulture.
        Strings.Resources.Culture = culture;
    }

    public static string? GetString(string key)
    {
        return Strings.Resources.ResourceManager.GetString(key);
    }
}