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
        bool IsDesktop();

        /// <summary>
        /// Returns true is the current
        /// device is Xbox or other device
        /// optimized for a 10-foot viewing
        /// distance.
        /// </summary>
        bool IsTenFoot();
    }
}
