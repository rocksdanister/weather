using Drizzle.Common.Services;
using Windows.ApplicationModel.Resources;

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
        throw new System.NotImplementedException();
    }
}
