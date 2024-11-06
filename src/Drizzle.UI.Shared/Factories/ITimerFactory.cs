using Drizzle.Common.Services;
using System;

namespace Drizzle.UI.Shared.Factories;

public interface ITimerFactory
{
    ITimerService CreateTimer();
}
