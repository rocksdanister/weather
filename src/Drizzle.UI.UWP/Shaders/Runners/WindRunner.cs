using ComputeSharp;
using ComputeSharp.Uwp;
using Drizzle.Common.Helpers;
using Drizzle.Models.Shaders;
using Drizzle.UI.UWP.Helpers;
using System;

namespace Drizzle.UI.Shaders.Runners;

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
        this.image = ComputeSharpUtil.CreateTextureOrPlaceholder(currentProperties.ImagePath, graphics);
        this.depth = ComputeSharpUtil.CreateTextureOrPlaceholder(currentProperties.DepthPath, graphics);
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
            this.image = ComputeSharpUtil.CreateTextureOrPlaceholder(currentProperties.ImagePath, texture.GraphicsDevice);
            this.depth = ComputeSharpUtil.CreateTextureOrPlaceholder(currentProperties.DepthPath, texture.GraphicsDevice);
        }

        UpdateProperties();
        // Adjust delta instead of actual time/speed to avoid rewinding time
        simulatedTime += (timespan.TotalSeconds - previousTime) * currentProperties.TimeMultiplier;
        previousTime = timespan.TotalSeconds;

        texture.GraphicsDevice.ForEach(texture, new Wind((float)simulatedTime,
            image,
            depth,
            new float4(currentProperties.Mouse.X, currentProperties.Mouse.Y, currentProperties.Mouse.W, currentProperties.Mouse.Z),
            mouseOffset,
            new float3(currentProperties.Color1.X, currentProperties.Color1.Y, currentProperties.Color1.Z),
            new float3(currentProperties.Color2.X, currentProperties.Color2.Y, currentProperties.Color2.Z),
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
        currentProperties.TimeMultiplier = MathUtil.Lerp(currentProperties.TimeMultiplier, properties().TimeMultiplier, 0.05f);
        currentProperties.Brightness = MathUtil.Lerp(currentProperties.Brightness, properties().Brightness, 0.05f);
        currentProperties.Saturation = MathUtil.Lerp(currentProperties.Saturation, properties().Saturation, 0.01f);
        // Mouse
        currentProperties.Mouse = properties().Mouse;
        currentProperties.MouseSpeed = properties().MouseSpeed;
        currentProperties.MouseInertia = properties().MouseInertia;
        mouseOffset.X += (currentProperties.MouseSpeed * currentProperties.Mouse.X - mouseOffset.X) * currentProperties.MouseInertia;
        mouseOffset.Y += (currentProperties.MouseSpeed * currentProperties.Mouse.Y - mouseOffset.Y) * currentProperties.MouseInertia;
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
