using Drizzle.Common.Services;

namespace Drizzle.UI.Avalonia.Services;

public class SystemInfoProvider : ISystemInfoProvider
{
    public bool IsDesktop() => true;

    public bool IsTenFoot() => false;
}
