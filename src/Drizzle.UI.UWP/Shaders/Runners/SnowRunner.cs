using ComputeSharp;
using ComputeSharp.Uwp;
using Drizzle.Common.Helpers;
using Drizzle.Models.Shaders;
using Drizzle.UI.UWP.Helpers;
using System;

namespace Drizzle.UI.Shaders.Runners;

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
        image = ComputeSharpUtil.CreateTextureOrPlaceholder(currentProperties.ImagePath, GraphicsDevice.GetDefault());
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
            this.image = ComputeSharpUtil.CreateTextureOrPlaceholder(currentProperties.ImagePath, texture.GraphicsDevice);
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
        currentProperties.Width = MathUtil.Lerp(currentProperties.Width, properties().Width, 0.01f);
        //currentProperties.Speed = MathUtil.Lerp(currentProperties.Speed, properties().Speed, 0.01f);
        //currentProperties.Depth = MathUtil.Lerp(currentProperties.Depth, properties().Depth, 0.01f);
        currentProperties.PostProcessing = MathUtil.Lerp(currentProperties.PostProcessing, properties().PostProcessing, 0.05f);
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
        currentProperties.Depth = properties().Depth;
        currentProperties.Speed = properties().Speed;
        currentProperties.Layers = properties().Layers;
        currentProperties.IsBlur = properties().IsBlur;
        currentProperties.IsLightning = properties().IsLightning;
        currentProperties.IsDaytime = properties().IsDaytime;
    }
}
