using Drizzle.Common.Services;
using Drizzle.Models.Weather;

namespace Drizzle.UI.Avalonia.Services;

public class SoundService : ISoundService
{
    public bool AutoPause { get; set; }
    public bool IsMuted { get; set; }
    public int Volume { get; set; }

    public void Play()
    {
        //TODO
    }

    public void Pause()
    {
        //TODO
    }

    public void SetSource(WmoWeatherCode code, bool isDaytime)
    {
        //TODO
    }
    public void Dispose()
    {
        //TODO
    }
}
