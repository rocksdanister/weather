using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.Common.Services
{
    public interface ICacheService
    {
        /// <summary>
        /// Most recent cache item access time.
        /// </summary>
        DateTime LastAccessTime { get; }
        /// <summary>
        /// Delete expired cache items
        /// </summary>
        void RemoveExpired();
        /// <summary>
        /// Delete all cache items
        /// </summary>
        void Clear();
        bool UseCache { get; set; }
        Task<byte[]> GetFileFromCacheAsync(string url, bool throwException = false);
        Task<byte[]> GetFileFromCacheAsync(Uri uri, bool throwException = false);
        Task<Stream> GetFileStreamFromCacheAsync(string url, bool throwException = false);
        Task<Stream> GetFileStreamFromCacheAsync(Uri uri, bool throwException = false);
    }
}
