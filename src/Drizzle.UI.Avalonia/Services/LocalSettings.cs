using Drizzle.Common.Constants;
using Drizzle.Common.Services;
using Drizzle.UI.Avalonia.Constants;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Drizzle.UI.Avalonia.Services;

public class LocalSettings : IUserSettings
{
    public event EventHandler<string>? SettingSet;

    private readonly string setingsPath;
    private readonly Dictionary<string, object> settings;

    public LocalSettings()
    {
        setingsPath = PathConstants.SettingsFile;
        settings = LoadSettings();
    }

    public T Get<T>(string settingKey)
    {
        if (settings.TryGetValue(settingKey, out var value))
        {
            // If the exact type then return.
            if (value is T typedValue)
                return typedValue;

            // Fix for numeric values being boxed as object in dictionary.
            // Ref: https://stackoverflow.com/questions/57107769/c-sharp-unable-to-cast-object-of-type-system-double-to-type-system-single
            if (typeof(T).IsPrimitive && value.GetType().IsPrimitive)
                return (T)Convert.ChangeType(value, typeof(T));

            throw new InvalidCastException($"The setting '{settingKey}' is not of type {typeof(T)}.");
        }

        return (T)UserSettingsConstants.Defaults[settingKey];
    }

    public T Get<T>(string settingKey, T defaultOverride)
    {
        return settings.TryGetValue(settingKey, out var value) ? (T)value! : defaultOverride;
    }

    public T? GetAndDeserialize<T>(string settingKey)
    {
        if (settings.TryGetValue(settingKey, out var value) && value is string jsonString)
            return JsonConvert.DeserializeObject<T>(jsonString);

        return (T)UserSettingsConstants.Defaults[settingKey];
    }

    public void Set<T>(string settingKey, T value)
    {
        settings[settingKey] = value!;
        SaveSettings();
        SettingSet?.Invoke(this, settingKey);
    }

    public void SetAndSerialize<T>(string settingKey, T value)
    {
        string jsonString = JsonConvert.SerializeObject(value);
        Set(settingKey, jsonString);
    }

    private Dictionary<string, object> LoadSettings()
    {
        if (File.Exists(setingsPath))
        {
            var json = File.ReadAllText(setingsPath);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? [];
        }
        return [];
    }

    private void SaveSettings()
    {
        var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        Directory.CreateDirectory(Path.GetDirectoryName(setingsPath)!);
        File.WriteAllText(setingsPath, json);
    }
}
