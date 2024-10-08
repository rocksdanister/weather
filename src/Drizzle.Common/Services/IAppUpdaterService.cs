using Drizzle.Models.Enums;
using System;
using System.Threading.Tasks;

namespace Drizzle.Common.Services;

public interface IAppUpdaterService
{
    /// <summary>
    /// Utc time since last update checked succesfully.
    /// </summary>
    DateTime LastChecked { get; }
    Task<AppUpdateStatus> CheckUpdateAsync();
}
