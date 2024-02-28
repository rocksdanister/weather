using Drizzle.Common;
using Drizzle.Common.Services;
using Drizzle.Models.Weather;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;

namespace Drizzle.UI.UWP.Services
{
    public class SoundService : ISoundService
    {
        private WmoWeatherCode currentAudiokey = (WmoWeatherCode)(-99);
        private bool currentIsDaytime = false;
        private readonly MediaPlayer mediaPlayer;
        private readonly ILogger logger;
        private bool disposedValue;
        private bool isUserPaused = false;

        public SoundService(ILogger<SoundService> logger)
        {
            this.logger = logger;

            mediaPlayer = new MediaPlayer()
            {
                AutoPlay = false,
                IsLoopingEnabled = true,
            };
            // Ref: https://learn.microsoft.com/en-us/windows/uwp/audio-video-camera/system-media-transport-controls
            mediaPlayer.CommandManager.IsEnabled = false;
            mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
        }

        private bool _autoPause;
        public bool AutoPause
        {
            get => _autoPause;
            set
            {
                _autoPause = value;
                if (Volume > 0 && !isUserPaused)
                    mediaPlayer.Play();
                else if (AutoPause && Volume == 0)
                    mediaPlayer.Pause();
            }
        }

        public int Volume
        {
            get => (int)(mediaPlayer.Volume * 100f);
            set
            {
                mediaPlayer.Volume = value / 100f;
                if (AutoPause)
                {
                    if (mediaPlayer.Volume > 0 && !isUserPaused)
                        mediaPlayer.Play();
                    else
                        mediaPlayer.Pause();
                }
            }
        }

        public bool IsMuted 
        { 
            get => mediaPlayer.IsMuted;
            set => mediaPlayer.IsMuted = value;
        }

        public void SetSource(WmoWeatherCode code, bool isDaytime)
        {
            if (currentAudiokey == code && isDaytime == currentIsDaytime)
                return;

            var newAudio = isDaytime ? dayAudioAssets[code] : nightAudioAssets[code];
            if (currentIsDaytime && dayAudioAssets.TryGetValue(currentAudiokey, out Uri dayUri) && dayUri == newAudio) {
                // Skip if same audio source already loaded
            }
            else if (!currentIsDaytime && nightAudioAssets.TryGetValue(currentAudiokey, out Uri nightUri) && nightUri == newAudio) {
                // Skip if same audio source already loaded
            }
            else {
                mediaPlayer.Source = MediaSource.CreateFromUri(newAudio);
            }
            currentAudiokey = code;
            currentIsDaytime = isDaytime;
        }

        public void Play()
        {
            isUserPaused = false;
            if (AutoPause && Volume == 0)
                return;

             mediaPlayer.Play();
        }

        public void Pause()
        {
            isUserPaused = true;
            mediaPlayer.Pause();
        }

        private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            logger.LogDebug(mediaPlayer.PlaybackSession.PlaybackState.ToString());
        }

        private void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            logger.LogError(args.Error.ToString());
        }

#pragma warning disable S1075 // URIs should not be hardcoded
        private static readonly IReadOnlyDictionary<WmoWeatherCode, Uri> dayAudioAssets = new Dictionary<WmoWeatherCode, Uri>()
        {
            { WmoWeatherCode.ClearSky, new Uri("ms-appx:///Assets/Sounds/forest.m4a") },
            { WmoWeatherCode.MainlyClear, new Uri("ms-appx:///Assets/Sounds/forest.m4a") },
            { WmoWeatherCode.PartlyCloudy, new Uri("ms-appx:///Assets/Sounds/wind.m4a") },
            { WmoWeatherCode.Overcast, new Uri("ms-appx:///Assets/Sounds/wind.m4a") },
            { WmoWeatherCode.Haze, new Uri("ms-appx:///Assets/Sounds/wind.m4a") },
            { WmoWeatherCode.Dust, new Uri("ms-appx:///Assets/Sounds/wind.m4a") },
            { WmoWeatherCode.Mist, new Uri("ms-appx:///Assets/Sounds/wind.m4a") },
            { WmoWeatherCode.Fog, new Uri("ms-appx:///Assets/Sounds/wind.m4a") },
            { WmoWeatherCode.DepositingRimeFog, new Uri("ms-appx:///Assets/Sounds/wind.m4a") },
            { WmoWeatherCode.LightDrizzle, new Uri("ms-appx:///Assets/Sounds/rain_light_2.m4a") },
            { WmoWeatherCode.ModerateDrizzle, new Uri("ms-appx:///Assets/Sounds/rain_light.m4a") },
            { WmoWeatherCode.DenseDrizzle, new Uri("ms-appx:///Assets/Sounds/rain.m4a") },
            { WmoWeatherCode.LightFreezingDrizzle, new Uri("ms-appx:///Assets/Sounds/rain_light.m4a") },
            { WmoWeatherCode.DenseFreezingDrizzle, new Uri("ms-appx:///Assets/Sounds/rain_freezing.m4a") },
            { WmoWeatherCode.SlightRain, new Uri("ms-appx:///Assets/Sounds/rain_light_2.m4a") },
            { WmoWeatherCode.ModerateRain, new Uri("ms-appx:///Assets/Sounds/rain_light.m4a") },
            { WmoWeatherCode.HeavyRain, new Uri("ms-appx:///Assets/Sounds/rain.m4a") },
            { WmoWeatherCode.LightFreezingRain, new Uri("ms-appx:///Assets/Sounds/rain_freezing.m4a") },
            { WmoWeatherCode.HeavyFreezingRain, new Uri("ms-appx:///Assets/Sounds/rain_freezing.m4a") },
            { WmoWeatherCode.SlightSnowFall, new Uri("ms-appx:///Assets/Sounds/snow.m4a") },
            { WmoWeatherCode.ModerateSnowFall, new Uri("ms-appx:///Assets/Sounds/snow.m4a") },
            { WmoWeatherCode.HeavySnowFall, new Uri("ms-appx:///Assets/Sounds/snow.m4a") },
            { WmoWeatherCode.SnowGrains, new Uri("ms-appx:///Assets/Sounds/rain_freezing.m4a") },
            { WmoWeatherCode.SlightRainShowers, new Uri("ms-appx:///Assets/Sounds/rain_light_2.m4a") },
            { WmoWeatherCode.ModerateRainShowers, new Uri("ms-appx:///Assets/Sounds/rain_light.m4a") },
            { WmoWeatherCode.ViolentRainShowers, new Uri("ms-appx:///Assets/Sounds/rain.m4a") },
            { WmoWeatherCode.SlightSnowShowers, new Uri("ms-appx:///Assets/Sounds/snow.m4a") },
            { WmoWeatherCode.HeavySnowShowers, new Uri("ms-appx:///Assets/Sounds/rain_freezing.m4a") },
            { WmoWeatherCode.Thunderstorm, new Uri("ms-appx:///Assets/Sounds/thunderstorm.m4a") },
            { WmoWeatherCode.ThunderstormLightHail, new Uri("ms-appx:///Assets/Sounds/rain_freezing_thunder.m4a") },
            { WmoWeatherCode.ThunderstormHeavyHail, new Uri("ms-appx:///Assets/Sounds/rain_freezing_thunder.m4a") }
        };

        private static readonly IReadOnlyDictionary<WmoWeatherCode, Uri> nightAudioAssets = new Dictionary<WmoWeatherCode, Uri>()
        {
            { WmoWeatherCode.ClearSky, new Uri("ms-appx:///Assets/Sounds/night.m4a") },
            { WmoWeatherCode.MainlyClear, new Uri("ms-appx:///Assets/Sounds/night.m4a") },
            { WmoWeatherCode.PartlyCloudy, new Uri("ms-appx:///Assets/Sounds/wind.m4a") },
            { WmoWeatherCode.Overcast, new Uri("ms-appx:///Assets/Sounds/wind.m4a") },
            { WmoWeatherCode.Haze, new Uri("ms-appx:///Assets/Sounds/wind.m4a") },
            { WmoWeatherCode.Dust, new Uri("ms-appx:///Assets/Sounds/wind.m4a") },
            { WmoWeatherCode.Mist, new Uri("ms-appx:///Assets/Sounds/wind.m4a") },
            { WmoWeatherCode.Fog, new Uri("ms-appx:///Assets/Sounds/wind.m4a") },
            { WmoWeatherCode.DepositingRimeFog, new Uri("ms-appx:///Assets/Sounds/wind.m4a") },
            { WmoWeatherCode.LightDrizzle, new Uri("ms-appx:///Assets/Sounds/rain_light_2.m4a") },
            { WmoWeatherCode.ModerateDrizzle, new Uri("ms-appx:///Assets/Sounds/rain_light.m4a") },
            { WmoWeatherCode.DenseDrizzle, new Uri("ms-appx:///Assets/Sounds/rain.m4a") },
            { WmoWeatherCode.LightFreezingDrizzle, new Uri("ms-appx:///Assets/Sounds/rain_light.m4a") },
            { WmoWeatherCode.DenseFreezingDrizzle, new Uri("ms-appx:///Assets/Sounds/rain_freezing.m4a") },
            { WmoWeatherCode.SlightRain, new Uri("ms-appx:///Assets/Sounds/rain_light_2.m4a") },
            { WmoWeatherCode.ModerateRain, new Uri("ms-appx:///Assets/Sounds/rain_light.m4a") },
            { WmoWeatherCode.HeavyRain, new Uri("ms-appx:///Assets/Sounds/rain.m4a") },
            { WmoWeatherCode.LightFreezingRain, new Uri("ms-appx:///Assets/Sounds/rain_freezing.m4a") },
            { WmoWeatherCode.HeavyFreezingRain, new Uri("ms-appx:///Assets/Sounds/rain_freezing.m4a") },
            { WmoWeatherCode.SlightSnowFall, new Uri("ms-appx:///Assets/Sounds/snow.m4a") },
            { WmoWeatherCode.ModerateSnowFall, new Uri("ms-appx:///Assets/Sounds/snow.m4a") },
            { WmoWeatherCode.HeavySnowFall, new Uri("ms-appx:///Assets/Sounds/snow.m4a") },
            { WmoWeatherCode.SnowGrains, new Uri("ms-appx:///Assets/Sounds/rain_freezing.m4a") },
            { WmoWeatherCode.SlightRainShowers, new Uri("ms-appx:///Assets/Sounds/rain_light_2.m4a") },
            { WmoWeatherCode.ModerateRainShowers, new Uri("ms-appx:///Assets/Sounds/rain_light.m4a") },
            { WmoWeatherCode.ViolentRainShowers, new Uri("ms-appx:///Assets/Sounds/rain.m4a") },
            { WmoWeatherCode.SlightSnowShowers, new Uri("ms-appx:///Assets/Sounds/snow.m4a") },
            { WmoWeatherCode.HeavySnowShowers, new Uri("ms-appx:///Assets/Sounds/rain_freezing.m4a") },
            { WmoWeatherCode.Thunderstorm, new Uri("ms-appx:///Assets/Sounds/thunderstorm.m4a") },
            { WmoWeatherCode.ThunderstormLightHail, new Uri("ms-appx:///Assets/Sounds/rain_freezing_thunder.m4a") },
            { WmoWeatherCode.ThunderstormHeavyHail, new Uri("ms-appx:///Assets/Sounds/rain_freezing_thunder.m4a") }
        };
#pragma warning restore S1075 // URIs should not be hardcoded

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    mediaPlayer?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SoundService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
