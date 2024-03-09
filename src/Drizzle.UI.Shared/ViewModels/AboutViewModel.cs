using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Common.Services;
using Drizzle.UI.UWP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.UI.UWP.ViewModels
{
    public sealed partial class AboutViewModel : ObservableObject
    {
        public AboutViewModel()
        {
            AppVersion = $"v{SystemInfoUtil.Instance.ApplicationVersion.Major}." +
                $"{SystemInfoUtil.Instance.ApplicationVersion.Minor}." +
                $"{SystemInfoUtil.Instance.ApplicationVersion.Build}." +
                $"{SystemInfoUtil.Instance.ApplicationVersion.Revision}";
        }

        [ObservableProperty]
        private string appVersion;
    }
}
