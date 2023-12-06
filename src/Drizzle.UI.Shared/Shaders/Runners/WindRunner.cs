using ComputeSharp;
using System;
using System.IO;
using Drizzle.UI.Shared.Shaders;
using Drizzle.UI.Shared.Shaders.Models;
using Drizzle.UI.Shared.Shaders.Helpers;
#if WINDOWS_UWP
using ComputeSharp.Uwp;
#else
using ComputeSharp.WinUI;
#endif

namespace Drizzle.UI.Shared.Shaders.Runners;

public class WindRunner : IShaderRunner
{
    private readonly Func<WindModel> properties;
    private readonly WindModel currentProperties;
    private ReadOnlyTexture2D<Rgba32, float4> image, depth;
    private float4 mouseOffset = float4.Zero;
    private double simulatedTime, previousTime;

    public WindRunner()
    {
        this.properties ??= () => new WindModel();
        this.currentProperties ??= new WindModel();

        var graphics = GraphicsDevice.GetDefault();
        this.image = ShaderUtil.CreateTextureOrPlaceholder(currentProperties.ImagePath, graphics);
        this.depth = ShaderUtil.CreateTextureOrPlaceholder(currentProperties.DepthPath, graphics);
    }

    public WindRunner(Func<WindModel> properties) : this()
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
        // Adjust delta instead of actual time/speed to avoid rewinding time
        simulatedTime += (timespan.TotalSeconds - previousTime) * currentProperties.TimeMultiplier;
        previousTime = timespan.TotalSeconds;

        texture.GraphicsDevice.ForEach(texture, new Wind((float)simulatedTime,
            image,
            depth,
            currentProperties.Mouse,
            mouseOffset,
            currentProperties.Color1,
            currentProperties.Color2,
            currentProperties.MaxSpeed - currentProperties.Speed + currentProperties.MinSpeed,
            currentProperties.Amplitude,
            new float2(currentProperties.ParallaxIntensityX, currentProperties.ParallaxIntensityY),
            currentProperties.Brightness,
            currentProperties.Saturation));

        return true;
    }

    private void UpdateProperties()
    {
        // Smoothing
        currentProperties.TimeMultiplier = ShaderUtil.Lerp(currentProperties.TimeMultiplier, properties().TimeMultiplier, 0.05f);
        currentProperties.Brightness = ShaderUtil.Lerp(currentProperties.Brightness, properties().Brightness, 0.05f);
        currentProperties.Saturation = ShaderUtil.Lerp(currentProperties.Saturation, properties().Saturation, 0.01f);
        // Mouse
        currentProperties.Mouse = properties().Mouse;
        mouseOffset.X += (-0.075f * currentProperties.Mouse.X - mouseOffset.X) * 0.08f;
        mouseOffset.Y += (-0.075f * currentProperties.Mouse.Y - mouseOffset.Y) * 0.08f;
        // Other
        currentProperties.Color1 = properties().Color1;
        currentProperties.Color2 = properties().Color2;
        currentProperties.Speed = properties().Speed;
        currentProperties.Amplitude = properties().Amplitude;
        currentProperties.ParallaxIntensityX = properties().ParallaxIntensityX;
        currentProperties.ParallaxIntensityY = properties().ParallaxIntensityY;
        currentProperties.IsDaytime = properties().IsDaytime;
    }
}
