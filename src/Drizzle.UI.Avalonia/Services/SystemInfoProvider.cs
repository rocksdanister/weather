﻿using Drizzle.Common.Services;
using System;
using System.Reflection;

namespace Drizzle.UI.Avalonia.Services;

public class SystemInfoProvider : ISystemInfoProvider
{
    private readonly string appVersionSettingKey = "AppVersion";
    private readonly string isFirstRunSettingKey = "IsFirstRun";
    private readonly IUserSettings userSettings;

    public SystemInfoProvider(IUserSettings userSettings)
    {
        this.userSettings = userSettings;

        IsFirstRun = DetectIfFirstUse();
        (IsAppUpdated, PreviousVersionInstalled) = DetectIfAppUpdated();
    }

    public bool IsDesktop => true;

    public bool IsTenFoot => false;

    public bool IsDirectX12Supported => true;

    public bool IsFirstRun { get; }

    public bool IsAppUpdated { get; }

    public string AppName => Assembly.GetEntryAssembly().GetName().Name;

    public Version AppVersion => Assembly.GetEntryAssembly().GetName().Version;

    public string GpuName => "Unknown";

    public Version PreviousVersionInstalled { get; }

    public (bool IsUpdated, Version PreviousVersion) DetectIfAppUpdated()
    {
        var currentVersion = AppVersion.ToString();

        // Get the previously installed version from settings
        var previousVersionString = userSettings.Get<string>(appVersionSettingKey, currentVersion);
        var previousVersion = Version.Parse(previousVersionString);

        if (previousVersionString != currentVersion)
        {
            // Update the stored version to the current version
            userSettings.Set(appVersionSettingKey, currentVersion);

            // App has been updated
            return (true, previousVersion);
        }
        return (false, previousVersion);
    }

    private bool DetectIfFirstUse()
    {
        if (userSettings.Get<bool>(isFirstRunSettingKey, true))
        {
            // After checking set it to false, we only need to check if the key exists.
            userSettings.Set(isFirstRunSettingKey, false);
            // If the key does not exists or its true (default override) then its first time.
            return true;
        }
        return false;
    }
}
