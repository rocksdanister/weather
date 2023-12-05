using Drizzle.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
