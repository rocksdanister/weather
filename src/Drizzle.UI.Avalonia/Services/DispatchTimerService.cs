using Avalonia.Threading;
using Drizzle.Common.Services;
using System;

namespace Drizzle.UI.Avalonia.Services;

public class DispatchTimerService : ITimerService
{
    public event EventHandler? TimerTick;

    private DispatcherTimer? timer;

    public void Start(TimeSpan interval)
    {
        Stop();
        timer = new DispatcherTimer
        {
            Interval = interval,
            IsEnabled = true
        };
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    public void Stop()
    {
        if (timer is null)
            return;

        timer.Tick -= Timer_Tick;
        timer.Stop();
        timer = null;
    }

    public bool IsRunning => timer?.IsEnabled ?? false;

    private void Timer_Tick(object? sender, EventArgs e)
    {
        TimerTick?.Invoke(sender, e);
    }
}
