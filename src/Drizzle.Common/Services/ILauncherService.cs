using System;
using System.Threading.Tasks;

namespace Drizzle.Common.Services
{
    public interface ILauncherService
    {
        public Task<bool> OpenBrowserAsync(Uri uri);
        public Task<bool> OpenBrowserAsync(string url);
        public Task<bool> OpenFileAsync(string filePath);
    }
}
