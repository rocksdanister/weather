using ComputeSharp;
using System;
using System.IO;
using Drizzle.UI.Shared.Shaders.Models;
using Drizzle.UI.Shared.Shaders.Helpers;
#if WINDOWS_UWP
using ComputeSharp.Uwp;
#else
using ComputeSharp.WinUI;
#endif

namespace Drizzle.UI.Shared.Shaders.Runners;

public sealed class DepthRunner : IShaderRunner
{
    private readonly Func<DepthModel> properties;
    private readonly DepthModel currentProperties;
    private ReadOnlyTexture2D<Rgba32, float4> image, depth;
    private float4 mouseOffset = float4.Zero;

    public DepthRunner()
    {
        this.properties ??= () => new DepthModel();
        this.currentProperties ??= new DepthModel();

        var graphics = GraphicsDevice.GetDefault();
        this.image = ShaderUtil.CreateTextureOrPlaceholder(currentProperties.ImagePath, graphics);
        this.depth = ShaderUtil.CreateTextureOrPlaceholder(currentProperties.DepthPath, graphics);
    }

    public DepthRunner(Func<DepthModel> properties) : this()
    {
        this.properties = properties;
        this.currentProperties = new(properties());
    }

    public bool TryExecute(IReadWriteNormalizedTexture2D<float4> texture, TimeSpan timespan, object parameter)
    {
        if (currentProperties.ImagePath != properties().ImagePath
            || currentProperties.DepthPath != properties().DepthPath
            || this.image is null 
            || this.depth is null
            || this.image.GraphicsDevice != texture.GraphicsDevice
            || this.depth.GraphicsDevice != texture.GraphicsDevice)
        {
            this.image?.Dispose();
            this.depth?.Dispose();
            currentProperties.ImagePath = properties().ImagePath;
            currentProperties.DepthPath = properties().DepthPath;
            this.image = ShaderUtil.CreateTextureOrPlaceholder(currentProperties.ImagePath, texture.GraphicsDevice);
            this.depth = ShaderUtil.CreateTextureOrPlaceholder(currentProperties.DepthPath, texture.GraphicsDevice);
        }

        UpdateProperties();
        texture.GraphicsDevice.ForEach(texture, new Depth(image,
            depth,
            currentProperties.Mouse,
            mouseOffset,
            new float2(currentProperties.IntensityX, currentProperties.IntensityY),
            currentProperties.IsBlur,
            currentProperties.Saturation,
            currentProperties.Brightness));

        return true;
    }

    private void UpdateProperties()
    {
        // Smoothing
        currentProperties.Brightness = ShaderUtil.Lerp(currentProperties.Brightness, properties().Brightness, 0.05f);
        currentProperties.Saturation = ShaderUtil.Lerp(currentProperties.Saturation, properties().Saturation, 0.01f);
        // Mouse
        currentProperties.Mouse = properties().Mouse;
        mouseOffset.X += (-0.075f * currentProperties.Mouse.X - mouseOffset.X) * 0.08f;
        mouseOffset.Y += (-0.075f * currentProperties.Mouse.Y - mouseOffset.Y) * 0.08f;
        // Other
        currentProperties.IntensityX = properties().IntensityX;
        currentProperties.IntensityY = properties().IntensityY;
        currentProperties.IsBlur = properties().IsBlur;
        currentProperties.IsDaytime = properties().IsDaytime;
    }
}
