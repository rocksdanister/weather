using Avalonia.Platform;
using Drizzle.Common.Helpers;
using Drizzle.Common.Services;
using Drizzle.Models;
using Drizzle.Models.Enums;
using Drizzle.Models.Weather;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drizzle.UI.Avalonia.Services;

public class AssetReader : IAssetReader
{
    // In this implementation we are returning uri file string instead of filepath.
    private readonly string assetsBaseUri = "avares://Drizzle.UI.Avalonia/Assets/Backgrounds/";
    private readonly List<ImageAssetModel> imageAssets;
    private readonly Random rnd = new();

    public AssetReader()
    {
        imageAssets = JsonUtil.Load<List<ImageAssetModel>>(AssetLoader.Open(new Uri($"{assetsBaseUri}Data.json")));
        foreach (var item in imageAssets)
        {
            item.FilePath = new Uri($"{assetsBaseUri}{item.FilePath.Replace("\\", "/")}", UriKind.Absolute).ToString();
            if (item.DepthPath != null)
                item.DepthPath = new Uri($"{assetsBaseUri}{item.DepthPath.Replace("\\", "/")}", UriKind.Absolute).ToString();
        }
    }

    public IEnumerable<ImageAssetModel> GetBackgrounds(ShaderTypes shader, bool isDaytime)
    {
        var hourRange = isDaytime ? Enumerable.Range(6, 12) : Enumerable.Range(18, 6).Concat(Enumerable.Range(0, 6));
        return GetBackgrounds(shader).Where(x => Array.Exists(x.Time, z => hourRange.Contains(z.Hour)));
    }

    public IEnumerable<ImageAssetModel> GetBackgrounds(ShaderTypes shader)
    {
        return imageAssets.Where(x => Array.Exists(x.WeatherCode, y => ((WmoWeatherCode)y).GetShader() == shader));
    }

    public ImageAssetModel GetRandomBackground(ShaderTypes shader, bool isDaytime)
    {
        // This shader has no texture input
        if (shader == ShaderTypes.clouds)
            return null;

        var selection = GetBackgrounds(shader, isDaytime);
        return selection.Any() ? selection.ElementAt(rnd.Next(selection.Count())) : null;
    }

    public ImageAssetModel GetRandomBackground(ShaderTypes shader)
    {
        if (shader == ShaderTypes.clouds)
            return null;

        var selection = GetBackgrounds(shader);
        return selection.Any() ? selection.ElementAt(rnd.Next(selection.Count())) : null;
    }
}
