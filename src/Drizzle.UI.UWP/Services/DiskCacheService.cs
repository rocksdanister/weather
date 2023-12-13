using Drizzle.Common.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Threading;

namespace Drizzle.UI.UWP.Services
{
    public class DiskCacheService : ICacheService
    {
        public bool UseCache { get; set; } = true;
        public DateTime LastAccessTime { get; private set; }

        private readonly HttpClient httpClient;
        private readonly TimeSpan duration;
        private readonly string cacheDir;

        public DiskCacheService(IHttpClientFactory httpClientFactory, string cacheDir, TimeSpan duration)
        {
            this.httpClient = httpClientFactory.CreateClient();
            this.cacheDir = cacheDir;
            this.duration = duration;

            InitializeInternal();
        }

        private void InitializeInternal()
        {
            Directory.CreateDirectory(cacheDir);
            InternalRemoveExpired();
        }

        public async Task<byte[]> GetFileFromCacheAsync(Uri uri, bool throwException = false)
        {
            try
            {
                var filePath = await GetFilePathFromCache(uri);
                return await File.ReadAllBytesAsync(filePath);
            }
            catch
            {
                if (throwException)
                    throw;

                return null;
            }
        }

        public async Task<byte[]> GetFileFromCacheAsync(string url, bool throwException = false)
        {
            Uri uri;
            try
            {
                uri = new Uri(url);
            }
            catch
            {
                if (throwException)
                    throw;

                return null;
            }
            return await GetFileFromCacheAsync(uri, throwException);
        }
            

        public async Task<Stream> GetFileStreamFromCacheAsync(Uri uri, bool throwException = false)
        {
            try
            {
                var filePath = await GetFilePathFromCache(uri);
                return File.OpenRead(filePath);
            }
            catch
            {
                if (throwException)
                    throw;

                return null;
            }
        }

        public async Task<Stream> GetFileStreamFromCacheAsync(string url, bool throwException = false)
        {
            Uri uri;
            try
            {
                uri = new Uri(url);
            }
            catch
            {
                if (throwException)
                    throw;

                return null;
            }
            return await GetFileStreamFromCacheAsync(uri, throwException);
        }

        private async Task<string> GetFilePathFromCache(Uri uri)
        {
            var fileName = GetCacheFileName(uri);
            var filePath = Path.Combine(cacheDir, fileName);
            var (isFileOutOfDateOrNotFound, lastAccessTime) = IsFileOutOfDateOrNotFoundWithLastAccesstime(filePath, duration);
            if (isFileOutOfDateOrNotFound || !UseCache)
            {
                var buffer = await httpClient.GetByteArrayAsync(uri);
                await File.WriteAllBytesAsync(filePath, buffer);
            }
            this.LastAccessTime = lastAccessTime is not null ? (DateTime)lastAccessTime : DateTime.Now;

            return filePath;
        }

        public void RemoveExpired()
        {
            InternalRemoveExpired();
        }

        private void InternalRemoveExpired()
        {
            DirectoryInfo dir = new(cacheDir);
            foreach (FileInfo file in dir.GetFiles())
            {
                if (IsFileOutOfDateOrNotFound(file.FullName, duration))
                {
                    try
                    {
                        file.Delete();
                    }
                    catch { }
                }
            }
        }

        public void Clear()
        {
            InternalClear();
        }

        private void InternalClear()
        {
            DirectoryInfo dir = new(cacheDir);
            foreach (FileInfo file in dir.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch { }
            }
        }

        private static bool IsFileOutOfDateOrNotFound(string file, TimeSpan duration)
        {
            if (string.IsNullOrWhiteSpace(file))
                return true;

            return !File.Exists(file) || DateTime.Now.Subtract(File.GetLastAccessTime(file)) > duration;
        }

        private static (bool, DateTime?) IsFileOutOfDateOrNotFoundWithLastAccesstime(string file, TimeSpan duration)
        {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
                return (true, null);

            var lastAccessTime = File.GetLastAccessTime(file);
            return (DateTime.Now.Subtract(lastAccessTime) > duration, lastAccessTime);
        }

        //Ref: https://github.com/CommunityToolkit/WindowsCommunityToolkit/blob/main/Microsoft.Toolkit.Uwp.UI/Cache/CacheBase.cs
        private static string GetCacheFileName(Uri uri)
        {
            return CreateHash64(uri.ToString()).ToString();
        }

        private static ulong CreateHash64(string str)
        {
            byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(str);

            ulong value = (ulong)utf8.Length;
            for (int n = 0; n < utf8.Length; n++)
            {
                value += (ulong)utf8[n] << ((n * 5) % 56);
            }

            return value;
        }
    }
}
