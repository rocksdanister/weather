using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace Drizzle.Common.Services;

public interface IFileService
{
    string LocalFolderPath { get; }
    string TempFolderPath { get; }
    string LogFolderPath { get; }
    string CachePath { get; }
    Task<(Stream stream, string fileName)> OpenFileAsync();
    Task<(Stream stream, string fileName)> OpenImageFileAsync();
}
