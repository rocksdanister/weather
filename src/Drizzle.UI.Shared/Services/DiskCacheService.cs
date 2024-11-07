using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Drizzle.Common.Services;

namespace Drizzle.UI.Shared.Services;

public class DiskCacheService : ICacheService
{
    public bool UseCache { get; set; } = true;
    public DateTime LastCachedTime { get; private set; }

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
        var fileInfo = GetFileStalenessInfo(filePath, duration);
        if (fileInfo.isStale || !UseCache)
        {
            var buffer = await httpClient.GetByteArrayAsync(uri);
            await File.WriteAllBytesAsync(filePath, buffer);
        }
        this.LastCachedTime = fileInfo.lastWriteTime is not null ? (DateTime)fileInfo.lastWriteTime : DateTime.Now;

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
            if (GetFileStalenessInfo(file.FullName, duration).isStale)
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

    private static (bool isStale, DateTime? lastWriteTime) GetFileStalenessInfo(string file, TimeSpan duration)
    {
        if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
            return (true, null);

        var lastWriteTime = File.GetLastWriteTime(file);
        return (DateTime.Now.Subtract(lastWriteTime) > duration, lastWriteTime);
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
