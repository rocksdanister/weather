using ComputeSharp;
using System;
using System.Diagnostics;
using System.IO;
using Drizzle.UI.Shared.Shaders.Models;
using Drizzle.UI.Shared.Shaders.Helpers;
#if WINDOWS_UWP
using ComputeSharp.Uwp;
#else
using ComputeSharp.WinUI;
#endif

namespace Drizzle.UI.Shared.Shaders.Runners;

public sealed class SnowRunner : IShaderRunner
{
    private readonly Func<SnowModel> properties;
    private readonly SnowModel currentProperties;
    private ReadOnlyTexture2D<Rgba32, float4> image;
    private float4 mouseOffset = float4.Zero;
    private double simulatedTime, previousTime;

    public SnowRunner()
    {
        this.properties ??= () => new SnowModel();
        this.currentProperties ??= new SnowModel();
        image = ShaderUtil.CreateTextureOrPlaceholder(currentProperties.ImagePath, GraphicsDevice.GetDefault());
    }

    public SnowRunner(Func<SnowModel> properties) : this()
    {
        this.properties = properties;
        this.currentProperties = new(properties());
    }

    public bool TryExecute(IReadWriteNormalizedTexture2D<float4> texture, TimeSpan timespan, object parameter)
    {
        if (currentProperties.ImagePath != properties().ImagePath || this.image is null || this.image.GraphicsDevice != texture.GraphicsDevice)
        {
            this.image?.Dispose();
            currentProperties.ImagePath = properties().ImagePath;
            this.image = ShaderUtil.CreateTextureOrPlaceholder(currentProperties.ImagePath, texture.GraphicsDevice);
        }

        UpdateProperties();
        // Adjust delta instead of actual time/speed to avoid rewinding time
        simulatedTime += (timespan.TotalSeconds - previousTime)*currentProperties.TimeMultiplier;
        previousTime = timespan.TotalSeconds;

        texture.GraphicsDevice.ForEach(texture, new Snow((float)simulatedTime,
            mouseOffset,
            currentProperties.Speed,
            currentProperties.Depth,
            currentProperties.Width,
            currentProperties.Brightness,
            currentProperties.Saturation,
            currentProperties.Layers,
            currentProperties.PostProcessing,
            currentProperties.IsLightning,
            currentProperties.IsBlur,
            image));

        return true;
    }

    private void UpdateProperties()
    {
        // Smoothing
        currentProperties.Width = ShaderUtil.Lerp(currentProperties.Width, properties().Width, 0.01f);
        //currentProperties.Speed = ShaderUtil.Lerp(currentProperties.Speed, properties().Speed, 0.01f);
        //currentProperties.Depth = ShaderUtil.Lerp(currentProperties.Depth, properties().Depth, 0.01f);
        currentProperties.PostProcessing = ShaderUtil.Lerp(currentProperties.PostProcessing, properties().PostProcessing, 0.05f);
        currentProperties.TimeMultiplier = ShaderUtil.Lerp(currentProperties.TimeMultiplier, properties().TimeMultiplier, 0.05f);
        currentProperties.Brightness = ShaderUtil.Lerp(currentProperties.Brightness, properties().Brightness, 0.05f);
        currentProperties.Saturation = ShaderUtil.Lerp(currentProperties.Saturation, properties().Saturation, 0.01f);
        // Mouse
        currentProperties.Mouse = properties().Mouse;
        currentProperties.Inertia = properties().Inertia;
        currentProperties.MoveSpeed = properties().MoveSpeed;
        mouseOffset.X += (currentProperties.MoveSpeed * currentProperties.Mouse.X - mouseOffset.X) * currentProperties.Inertia;
        mouseOffset.Y += (currentProperties.MoveSpeed * currentProperties.Mouse.Y - mouseOffset.Y) * currentProperties.Inertia;
        // Other
        currentProperties.Depth = properties().Depth;
        currentProperties.Speed = properties().Speed;
        currentProperties.Layers = properties().Layers;
        currentProperties.IsBlur = properties().IsBlur;
        currentProperties.IsLightning = properties().IsLightning;
        currentProperties.IsDaytime = properties().IsDaytime;
    }
}
