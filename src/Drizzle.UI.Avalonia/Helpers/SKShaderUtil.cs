using Avalonia.Platform;
using Drizzle.Models.Enums;
using SkiaSharp;
using System;
using System.IO;
using System.Numerics;

namespace Drizzle.UI.Avalonia.Helpers;

public static class SKShaderUtil
{
    public static (SKShader? imageShader, Vector3 dimensions) CreateImageShader(Uri assetUri, SKShaderTileMode wrap = SKShaderTileMode.Clamp)
    {
        if (assetUri is null)
            return (null, Vector3.Zero);

        SKImage? image = null;

        try
        {
            if (assetUri.IsFile)
            {
                var filePath = assetUri.LocalPath;
                using var stream = File.OpenRead(filePath);
                image = SKImage.FromEncodedData(stream);
            }
            else
            {
                // Assume Avalonia embedded asset
                image = SKImage.FromEncodedData(AssetLoader.Open(assetUri));
            }

            var dimensions = image != null ? new Vector3(image.Width, image.Height, 0) : Vector3.Zero;
            return new(image?.ToShader(wrap, wrap), dimensions);
        }
        finally
        {
            image?.Dispose();
        }
    }

    public static SKShaderTileMode WrapToSkTile(TextureWrapMode wrap)
    {
        return wrap switch
        {
            TextureWrapMode.clamp => SKShaderTileMode.Clamp,
            TextureWrapMode.repeat => SKShaderTileMode.Repeat,
            TextureWrapMode.mirror => SKShaderTileMode.Mirror,
            _ => SKShaderTileMode.Clamp,
        };
    }
}
