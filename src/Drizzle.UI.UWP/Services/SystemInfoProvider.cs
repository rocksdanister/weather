using Drizzle.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Profile;

namespace Drizzle.UI.UWP.Services
{
    public class SystemInfoProvider : ISystemInfoProvider
    {
        public bool IsDesktop() => App.IsDesktop;

        public bool IsTenFoot() => App.IsTenFoot;
    }
}
