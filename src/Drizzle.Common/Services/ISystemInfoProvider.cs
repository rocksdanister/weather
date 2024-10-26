using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Common.Services
{
    public interface ISystemInfoProvider
    {
        /// <summary>
        /// Returns true is the current device is a Desktop.
        /// </summary>
        bool IsDesktop { get; }

        /// <summary>
        /// Returns true is the current
        /// device is Xbox or other device
        /// optimized for a 10-foot viewing
        /// distance.
        /// </summary>
        bool IsTenFoot { get; }

        bool IsHardwareAccelerated { get; }

        public bool IsFirstRun { get; }

        public bool IsAppUpdated { get; }

        public string AppName { get; }

        public Version AppVersion { get; }

        public string GpuName { get; }
    }
}
