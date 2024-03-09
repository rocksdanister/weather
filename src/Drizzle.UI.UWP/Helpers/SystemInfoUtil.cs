using CommunityToolkit.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System.UserProfile;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Ref: https://github.com/CommunityToolkit/WindowsCommunityToolkit/blob/main/Microsoft.Toolkit.Uwp/Helpers/SystemInformation.cs
namespace Drizzle.UI.UWP.Helpers
{
    public sealed class SystemInfoUtil
    {
        public SystemInfoUtil()
        {
            ApplicationName = Package.Current.DisplayName;
            ApplicationVersion = Package.Current.Id.Version;

            try
            {
                Culture = GlobalizationPreferences.Languages.Count > 0 ? new CultureInfo(GlobalizationPreferences.Languages.First()) : null;
            }
            catch
            {
                Culture = null;
            }

            IsFirstRun = DetectIfFirstUse();
            (IsAppUpdated, PreviousVersionInstalled) = DetectIfAppUpdated();
        }

        public static SystemInfoUtil Instance { get; } = new();

        /// <summary>
        /// Gets a value indicating whether the app is being used for the first time since it was installed.
        /// Use this to tell if you should do or display something different for the app's first use.
        /// </summary>
        public bool IsFirstRun { get; }

        /// <summary>
        /// Gets a value indicating whether the app is being used for the first time since being upgraded from an older version.
        /// Use this to tell if you should display details about what has changed.
        /// </summary>
        public bool IsAppUpdated { get; }

        /// <summary>
        /// Gets the application's name.
        /// </summary>
        public string ApplicationName { get; }

        /// <summary>
        /// Gets the application's version.
        /// </summary>
        public PackageVersion ApplicationVersion { get; }

        /// <summary>
        /// Gets the previous version of the app that was installed.
        /// This will be the current version if a previous version of the app was installed
        /// before using <see cref="SystemInformation"/> or if the app is not updated.
        /// </summary>
        public PackageVersion PreviousVersionInstalled { get; }

        /// <summary>
        /// Gets the user's most preferred culture.
        /// </summary>
        public CultureInfo Culture { get; }

        private bool DetectIfFirstUse()
        {
            if (KeyExists(nameof(IsFirstRun)))
            {
                return false;
            }

            SaveKey(nameof(IsFirstRun), true);

            return true;
        }

        private (bool IsUpdated, PackageVersion PreviousVersion) DetectIfAppUpdated()
        {
            var currentVersion = ApplicationVersion.ToFormattedString();

            // If the "currentVersion" key does not exist, it means that this is the first time this method
            // is ever called. That is, this is either the first time the app has been launched, or the first
            // time a previously existing app has run this method (or has run it after a new update of the app).
            // In this case, save the current version and report the same version as previous version installed.
            if (!KeyExists(nameof(currentVersion)))
            {
                SaveKey(nameof(currentVersion), currentVersion);
            }
            else
            {
                var previousVersion = ReadKey<string>(nameof(currentVersion));

                // There are two possible cases if the "currentVersion" key exists:
                //   1) The previous version is different than the current one. This means that the application
                //      has been updated since the last time this method was called. We can overwrite the saved
                //      setting for "currentVersion" to bring that value up to date, and return its old value.
                //   2) The previous version matches the current one: the app has just been reopened without updates.
                //      In this case we have nothing to do and just return the previous version installed to be the same.
                if (currentVersion != previousVersion)
                {
                    SaveKey(nameof(currentVersion), currentVersion);

                    return (true, previousVersion.ToPackageVersion());
                }
            }

            return (false, currentVersion.ToPackageVersion());
        }

        private static bool KeyExists(string key)
        {
            return ApplicationData.Current.LocalSettings.Values.ContainsKey(key);
        }

        private static void SaveKey<T>(string key, T value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = Serialize(value);
        }

        private static T? ReadKey<T>(string key, T? @default = default)
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var valueObj) && valueObj is string valueString)
            {
                return Deserialize<T>(valueString);
            }

            return @default;
        }

        private static T Deserialize<T>(object value)
        {
            var type = typeof(T);
            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsPrimitive || type == typeof(string))
            {
                return (T)Convert.ChangeType(value, type);
            }

            return ThrowNotSupportedException();

            static T ThrowNotSupportedException() => throw new NotSupportedException("This serializer can only handle primitive types and strings. Please implement your own IObjectSerializer for more complex scenarios.");
        }

        private static object Serialize<T>(T value)
        {
            return value;
        }
    }
}
