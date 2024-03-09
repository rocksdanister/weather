using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Common
{
    public enum ShaderTypes
    {
        clouds,
        rain,
        snow,
        depth,
        fog,
        tunnel = 100,
    }

    public enum AppPerformance
    {
        potato,
        performance,
        quality,
        dynamic
    }

    public enum AppTheme
    {
        auto,
        dark,
        light
    }
}
