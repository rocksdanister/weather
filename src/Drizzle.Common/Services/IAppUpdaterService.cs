using Drizzle.Models.Enums;
using System;
using System.Threading.Tasks;

namespace Drizzle.Common.Services;

public interface IAppUpdaterService
{
    /// <summary>
    /// Utc time since last update checked succesfully.
    /// </summary>
    DateTime LastCheckedTime { get; }
    AppUpdateStatus LastCheckedStatus { get; }
    Task<AppUpdateStatus> CheckUpdateAsync();
    void Start();

    event EventHandler<AppUpdateStatus> UpdateChecked;
}
