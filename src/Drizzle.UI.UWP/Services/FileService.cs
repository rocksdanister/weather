using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Drizzle.Common.Services;
using Windows.Storage;

namespace Drizzle.UI.UWP.Services
{
    public class FileService : IFileService
    {
        public string LocalFolderPath => ApplicationData.Current.LocalFolder.Path;

        public string CachePath => Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "Cache");

        public string TempFolderPath => ApplicationData.Current.TemporaryFolder.Path;

        public async Task<(Stream, string)> OpenFileAsync()
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add("*");

            var file = await picker.PickSingleFileAsync();
            return file != null ? (await file.OpenStreamForReadAsync(), file.Name) : (null, null);
        }

        public async Task<(Stream, string)> OpenImageFileAsync()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();
            return file != null ? (await file.OpenStreamForReadAsync(), file.Name) : (null, null);
        }
    }
}
