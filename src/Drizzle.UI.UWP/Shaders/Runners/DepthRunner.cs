using ComputeSharp;
using ComputeSharp.Uwp;
using Drizzle.Common.Helpers;
using Drizzle.Models.Shaders;
using Drizzle.UI.UWP.Helpers;
using System;

namespace Drizzle.UI.Shaders.Runners;

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
        this.image = ComputeSharpUtil.CreateTextureOrPlaceholder(currentProperties.ImagePath, graphics);
        this.depth = ComputeSharpUtil.CreateTextureOrPlaceholder(currentProperties.DepthPath, graphics);
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
            this.image = ComputeSharpUtil.CreateTextureOrPlaceholder(currentProperties.ImagePath, texture.GraphicsDevice);
            this.depth = ComputeSharpUtil.CreateTextureOrPlaceholder(currentProperties.DepthPath, texture.GraphicsDevice);
        }

        UpdateProperties();
        texture.GraphicsDevice.ForEach(texture, new Depth(image,
            depth,
            new float4(currentProperties.Mouse.X, currentProperties.Mouse.Y, currentProperties.Mouse.W, currentProperties.Mouse.Z),
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
        currentProperties.Brightness = MathUtil.Lerp(currentProperties.Brightness, properties().Brightness, 0.05f);
        currentProperties.Saturation = MathUtil.Lerp(currentProperties.Saturation, properties().Saturation, 0.01f);
        // Mouse
        currentProperties.Mouse = properties().Mouse;
        currentProperties.MouseSpeed = properties().MouseSpeed;
        currentProperties.MouseInertia = properties().MouseInertia;
        mouseOffset.X += (currentProperties.MouseSpeed * currentProperties.Mouse.X - mouseOffset.X) * currentProperties.MouseInertia;
        mouseOffset.Y += (currentProperties.MouseSpeed * currentProperties.Mouse.Y - mouseOffset.Y) * currentProperties.MouseInertia;
        // Other
        currentProperties.IntensityX = properties().IntensityX;
        currentProperties.IntensityY = properties().IntensityY;
        currentProperties.IsBlur = properties().IsBlur;
        currentProperties.IsDaytime = properties().IsDaytime;
    }
}
