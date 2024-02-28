using Drizzle.Models.Weather;
using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Common.Services
{
    public interface ISoundService : IDisposable
    {
        /// <summary>
        /// Automatically pause playback when volume is set to 0
        /// </summary>
        public bool AutoPause { get; set; }
        public bool IsMuted { get; set; }
        public int Volume { get; set; }
        public void SetSource(WmoWeatherCode code, bool isDaytime);
        public void Play();
        public void Pause();
    }
}