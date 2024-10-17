using Avalonia.Platform.Storage;
using Drizzle.Common.Services;
using Drizzle.UI.Avalonia.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Drizzle.UI.Avalonia.Services
{
    public class FileService : IFileService
    {
        private readonly INavigator navigator;

        public FileService(INavigator navigator)
        {
            this.navigator = navigator;

            // Create if not exists
            Directory.CreateDirectory(LocalFolderPath);
            Directory.CreateDirectory(TempFolderPath);
        }

        public string LocalFolderPath => GetLocalFolderPath();

        public string CachePath => GetCachePath();

        public string TempFolderPath => GetTempFolderPath();

        public string LogFolderPath => Path.Combine(GetLocalFolderPath(), "Logs");

        public async Task<(Stream stream, string fileName)> OpenFileAsync()
        {
            var mainWindow = navigator.RootFrame as MainWindow;
            var files = await mainWindow!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false      
            });

            return files.Count >= 1 ? (await files[0].OpenReadAsync(), files[0].Name) : (null, null);
        }

        public async Task<(Stream stream, string fileName)> OpenImageFileAsync()
        {
            var mainWindow = navigator.RootFrame as MainWindow;
            var files = await mainWindow!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = [FilePickerFileTypes.ImageAll]
            });

            return files.Count >= 1 ? (await files[0].OpenReadAsync(), files[0].Name) :(null, null);
        }

        private static string GetCachePath()
        {
            if (OperatingSystem.IsLinux())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "Lively Weather", "Cache");
            else if (OperatingSystem.IsMacOS())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Caches", "Lively Weather");
            else if (OperatingSystem.IsWindows())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Lively Weather", "Cache");
            throw new NotImplementedException();
        }

        private static string GetLocalFolderPath()
        {
            if (OperatingSystem.IsLinux())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "Lively Weather");
            else if (OperatingSystem.IsMacOS())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Preferences", "Lively Weather");
            else if (OperatingSystem.IsWindows())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Lively Weather");
            else
                throw new NotImplementedException();
        }

        private static string GetTempFolderPath()
        {
            if (OperatingSystem.IsLinux())
                return Path.Combine(Path.GetTempPath(), "Lively Weather");
            else if (OperatingSystem.IsMacOS())
                return Path.Combine(Path.GetTempPath(), "Lively Weather");
            else if (OperatingSystem.IsWindows())
                return Path.Combine(Path.GetTempPath(), "Lively Weather");
            else
                throw new NotImplementedException();
        }
    }
}
