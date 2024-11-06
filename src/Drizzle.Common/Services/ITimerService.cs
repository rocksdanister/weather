using System;

namespace Drizzle.Common.Services;

public interface ITimerService
{
    event EventHandler TimerTick;
    void Start(TimeSpan interval);
    void Stop();
    bool IsRunning { get; }
}
