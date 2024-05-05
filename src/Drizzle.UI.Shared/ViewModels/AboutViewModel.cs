using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        [RelayCommand]
        private async Task OpenPersonalWebsite()
        {
            await LinkUtil.OpenBrowser("https://rocksdanister.com");
        }

        [RelayCommand]
        private async Task OpenGithub()
        {
            await LinkUtil.OpenBrowser("https://github.com/rocksdanister");
        }

        [RelayCommand]
        private async Task OpenTwitter()
        {
            await LinkUtil.OpenBrowser("https://twitter.com/rocksdanister");
        }

        [RelayCommand]
        private async Task OpenYoutube()
        {
            await LinkUtil.OpenBrowser("https://www.youtube.com/channel/UClep84ofxC41H8-R9UfNPSQ");
        }

        [RelayCommand]
        private async Task OpenReddit()
        {
            await LinkUtil.OpenBrowser("https://reddit.com/u/rocksdanister");
        }

        [RelayCommand]
        private async Task OpenEmail()
        {
            await LinkUtil.OpenBrowser("mailto:awoo.git@gmail.com");
        }
    }
}
