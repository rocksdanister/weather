using Drizzle.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Drizzle.UI.Shared.Factories;

public class TimerFactory : ITimerFactory
{
    private readonly IServiceProvider serviceProvider;

    public TimerFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public ITimerService CreateTimer()
    {
        return serviceProvider.GetRequiredService<ITimerService>();
    }
}
