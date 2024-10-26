using NLog.Config;

namespace Drizzle.UI.Shared.Factories;

public interface INLogConfigFactory
{
    LoggingConfiguration Create(string baseDir);
}