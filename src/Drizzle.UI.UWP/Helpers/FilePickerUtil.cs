using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace Drizzle.UI.UWP.Helpers
{
    public static class FilePickerUtil
    {
        public static async Task<StorageFile> ShowImageDialogAsync()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            return await picker.PickSingleFileAsync();
        }
    }
}
