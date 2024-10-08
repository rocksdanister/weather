using Drizzle.Common.Services;
using Drizzle.Models.Enums;
using System;
using System.Threading.Tasks;

namespace Drizzle.UI.UWP.Services;

public class MicrosoftStoreUpdaterService : IAppUpdaterService
{
    public DateTime LastChecked => DateTime.MinValue;

    public Task<AppUpdateStatus> CheckUpdateAsync()
    {
        return Task.FromResult(AppUpdateStatus.notchecked);
    }
}
