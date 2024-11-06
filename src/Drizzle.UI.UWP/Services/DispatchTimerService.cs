using Drizzle.Common.Services;
using System;
using Windows.UI.Xaml;

namespace Drizzle.UI.UWP.Services;

public class DispatchTimerService : ITimerService
{
    public event EventHandler TimerTick;

    private DispatcherTimer timer;

    public void Start(TimeSpan interval)
    {
        Stop();
        timer = new DispatcherTimer
        {
            Interval = interval,
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

    public bool IsRunning => throw new NotImplementedException();

    private void Timer_Tick(object sender, object e)
    {
        TimerTick?.Invoke(sender, EventArgs.Empty);
    }
}
