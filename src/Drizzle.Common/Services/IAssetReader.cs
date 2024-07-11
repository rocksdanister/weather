using Drizzle.Models;
using Drizzle.Models.Enums;
using System.Collections.Generic;

namespace Drizzle.Common.Services
{
    public interface IAssetReader
    {
        IEnumerable<ImageAssetModel> GetBackgrounds(ShaderTypes shader);
        IEnumerable<ImageAssetModel> GetBackgrounds(ShaderTypes shader, bool isDaytime);
        ImageAssetModel GetRandomBackground(ShaderTypes shader, bool isDaytime);
        ImageAssetModel GetRandomBackground(ShaderTypes shader);
    }
}
