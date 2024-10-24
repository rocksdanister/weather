using Drizzle.Common.Services;
using System.Globalization;

namespace Drizzle.UI.Avalonia.Services;

public class ResourceService : IResourceService
{
    public string GetString(string resource)
    {
        // Compatibility with UWP .resw shared classes.
        var formattedResource = resource.Replace("/", ".");
        return Strings.Resources.ResourceManager.GetString(formattedResource);
    }

    public void SetCulture(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        var culture = new CultureInfo(name);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        // ResourceManager.GetString ignores this and uses CurrentUICulture.
        Strings.Resources.Culture = culture;
    }

    public void SetSystemDefaultCulture()
    {
        // Nothing to do here.
    }
}
