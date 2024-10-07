namespace Drizzle.Common.Services;

public interface IResourceService
{
    string GetString(string resource);
    void SetCulture(string name);
}
