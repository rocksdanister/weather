using NLog.Config;
using NLog.Targets;
using System.IO;

namespace Drizzle.UI.Shared.Factories;

// Ref: https://github.com/NLog/readthedocs/blob/master/docs/Configuration-API.md?plain=1
public class NLogConfigFactory : INLogConfigFactory
{
    public LoggingConfiguration Create(string baseDir)
    {
        var logConfig = new LoggingConfiguration();
        var fileTarget = new FileTarget("logfile")
        {
            FileName = Path.Combine(baseDir, "${cached:cached=true:inner=${date:format=yyyyMMdd_HHmmss}}.txt"),
            MaxArchiveFiles = 4,
            ArchiveAboveSize = 50000000
        };
        var consoleTarget = new ConsoleTarget("logconsole");
        var debuggerTarget = new DebuggerTarget("debugger")
        {
            Layout = "${logger}::${message}"
        };

        // Add targets to the configuration
        logConfig.AddTarget(fileTarget);
        logConfig.AddTarget(consoleTarget);
        logConfig.AddTarget(debuggerTarget);

        // Define rules
        logConfig.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, consoleTarget);
        logConfig.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, fileTarget);
        logConfig.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, debuggerTarget);

        return logConfig;
    }
}
