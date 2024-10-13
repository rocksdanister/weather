using ComputeSharp;
using ComputeSharp.Uwp;
using Drizzle.Common.Helpers;
using Drizzle.Models.Shaders;
using Drizzle.UI.Shared.Extensions;
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
        UpdateUniforms(texture.GraphicsDevice, timespan);
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

    private void UpdateUniforms(GraphicsDevice device, TimeSpan timespan)
    {
        // Textures
        if (currentProperties.ImagePath != properties().ImagePath || this.image is null || this.image.GraphicsDevice != device)
        {
            this.image?.Dispose();

            this.image = ComputeSharpUtil.CreateTextureOrPlaceholder(properties().ImagePath, device);
        }

        // Mouse
        currentProperties.Mouse = properties().Mouse;
        currentProperties.MouseSpeed = properties().MouseSpeed;
        currentProperties.MouseInertia = properties().MouseInertia;
        mouseOffset.X += (currentProperties.MouseSpeed * currentProperties.Mouse.X - mouseOffset.X) * currentProperties.MouseInertia;
        mouseOffset.Y += (currentProperties.MouseSpeed * currentProperties.Mouse.Y - mouseOffset.Y) * currentProperties.MouseInertia;

        // Time, adjust delta instead of actual time/speed to avoid rewinding time
        currentProperties.TimeMultiplier = MathUtil.Lerp(currentProperties.TimeMultiplier, properties().TimeMultiplier, 0.05f);
        simulatedTime += (timespan.TotalSeconds - previousTime) * currentProperties.TimeMultiplier;
        previousTime = timespan.TotalSeconds;

        // Shader specific
        properties().UniformFrameUpdate(currentProperties);
    }
}

