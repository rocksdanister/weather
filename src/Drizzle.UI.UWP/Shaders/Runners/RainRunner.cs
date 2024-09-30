using ComputeSharp;
using ComputeSharp.Uwp;
using Drizzle.Common.Helpers;
using Drizzle.Models.Shaders;
using Drizzle.UI.UWP.Helpers;
using System;

namespace Drizzle.UI.Shaders.Runners;

public sealed class RainRunner : IShaderRunner
{
    private readonly Func<RainModel> properties;
    private readonly RainModel currentProperties;
    private ReadOnlyTexture2D<Rgba32, float4> image;
    private float4 mouseOffset = float4.Zero;
    private double simulatedTime, previousTime;

    public RainRunner()
    {
        this.properties ??= () => new RainModel();
        this.currentProperties ??= new RainModel();
        image = ComputeSharpUtil.CreateTextureOrPlaceholder(currentProperties.ImagePath, GraphicsDevice.GetDefault());
    }

    public RainRunner(Func<RainModel> properties) : this()
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
            this.image = ComputeSharpUtil.CreateTextureOrPlaceholder(currentProperties.ImagePath, texture.GraphicsDevice);
        }

        UpdateProperties();
        // Adjust delta instead of actual time/speed to avoid rewinding time
        simulatedTime += (timespan.TotalSeconds - previousTime) * currentProperties.TimeMultiplier;
        previousTime = timespan.TotalSeconds;

        texture.GraphicsDevice.ForEach(texture, new Rain((float)simulatedTime,
            mouseOffset,
            currentProperties.Speed,
            currentProperties.Intensity,
            currentProperties.Zoom,
            currentProperties.Normal,
            currentProperties.Brightness,
            currentProperties.Saturation,
            currentProperties.PostProcessing,
            currentProperties.IsPanning,
            currentProperties.IsFreezing,
            currentProperties.IsLightning,
            image));

        return true;
    }

    private void UpdateProperties()
    {
        // Smoothing, value is increased by small step % every frame
        currentProperties.Intensity = MathUtil.Lerp(currentProperties.Intensity, properties().Intensity, 0.05f);
        currentProperties.Zoom = MathUtil.Lerp(currentProperties.Zoom, properties().Zoom, 0.1f);
        currentProperties.Normal = MathUtil.Lerp(currentProperties.Normal, properties().Normal, 0.05f);
        currentProperties.PostProcessing = MathUtil.Lerp(currentProperties.PostProcessing, properties().PostProcessing, 0.05f);
        currentProperties.Brightness = MathUtil.Lerp(currentProperties.Brightness, properties().Brightness, 0.05f);
        currentProperties.Saturation = MathUtil.Lerp(currentProperties.Saturation, properties().Saturation, 0.01f);
        currentProperties.TimeMultiplier = MathUtil.Lerp(currentProperties.TimeMultiplier, properties().TimeMultiplier, 0.05f);
        // Mouse
        currentProperties.Mouse = properties().Mouse;
        currentProperties.MouseSpeed = properties().MouseSpeed;
        currentProperties.MouseInertia = properties().MouseInertia;
        mouseOffset.X += (currentProperties.MouseSpeed * currentProperties.Mouse.X - mouseOffset.X) * currentProperties.MouseInertia;
        mouseOffset.Y += (currentProperties.MouseSpeed * currentProperties.Mouse.Y - mouseOffset.Y) * currentProperties.MouseInertia;
        // Other
        currentProperties.IsFreezing = properties().IsFreezing;
        currentProperties.IsLightning = properties().IsLightning;
        currentProperties.IsPanning = properties().IsPanning;
        currentProperties.Speed = properties().Speed;
        currentProperties.IsDaytime = properties().IsDaytime;
    }
}

