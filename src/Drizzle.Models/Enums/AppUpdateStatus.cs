namespace Drizzle.Models.Enums;

public enum AppUpdateStatus
{
    /// <summary>
    /// Software is up-to-date.
    /// </summary>
    uptodate,
    /// <summary>
    /// Update available.
    /// </summary>
    available,
    /// <summary>
    /// Installed software version higher than whats available online.
    /// </summary>
    invalid,
    /// <summary>
    /// Update not checked.
    /// </summary>
    notchecked,
    /// <summary>
    /// Update check failed
    /// </summary>
    error,
}