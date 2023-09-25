using Drizzle.Common.Constants;
using Drizzle.Common.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Drizzle.UI.UWP.Services;

/// <summary>
/// Class for storing and retrieving
/// user settings for application data
/// settings storage.
/// </summary>
public class LocalSettings : IUserSettings
{
    /// <inheritdoc/>
    public event EventHandler<string>? SettingSet;

    /// <inheritdoc/>
    public T Get<T>(string settingKey)
    {
        object result = ApplicationData.Current.LocalSettings.Values[settingKey];
        return result is null ? (T)UserSettingsConstants.Defaults[settingKey] : (T)result;
    }

    /// <inheritdoc/>
    public void Set<T>(string settingKey, T value)
    {
        ApplicationData.Current.LocalSettings.Values[settingKey] = value;
        SettingSet?.Invoke(this, settingKey);
    }

    /// <inheritdoc/>
    public T? GetAndDeserialize<T>(string settingKey)
    {
        object result = ApplicationData.Current.LocalSettings.Values[settingKey];
        if (result is string serialized)
        {
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        return (T)UserSettingsConstants.Defaults[settingKey];
    }

    /// <inheritdoc/>
    public void SetAndSerialize<T>(string settingKey, T value)
    {
        var serialized = JsonConvert.SerializeObject(value);
        Set(settingKey, serialized);
    }


    /// <inheritdoc/>
    public T Get<T>(string settingKey, T defaultOverride)
    {
        object result = ApplicationData.Current.LocalSettings.Values[settingKey];
        return result is null ? defaultOverride : (T)result;
    }
}
