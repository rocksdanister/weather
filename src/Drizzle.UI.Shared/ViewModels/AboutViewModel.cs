using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Common.Services;
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
            //AppVersion = $"v{SystemInformation.Instance.ApplicationVersion.Major}.{SystemInformation.Instance.ApplicationVersion.Minor}.{SystemInformation.Instance.ApplicationVersion.Build}.{SystemInformation.Instance.ApplicationVersion.Revision}";
        }

        [ObservableProperty]
        private string appVersion;
    }
}
