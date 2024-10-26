using Drizzle.Common.Services;
using Drizzle.Models.Enums;
using System;
using System.Threading.Tasks;

namespace Drizzle.UI.UWP.Services;

public class MicrosoftStoreUpdaterService : IAppUpdaterService
{
    public DateTime LastCheckedTime => DateTime.MinValue;
    public AppUpdateStatus LastCheckedStatus => AppUpdateStatus.notchecked;
    public event EventHandler<AppUpdateStatus> UpdateChecked;

    public Task<AppUpdateStatus> CheckUpdateAsync()
    {
        return Task.FromResult(AppUpdateStatus.notchecked);
    }

    public void Start()
    {
        // Nothing for now.
    }
}
