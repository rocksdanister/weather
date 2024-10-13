using ComputeSharp;
using ComputeSharp.Uwp;
using Drizzle.Common.Helpers;
using Drizzle.Models.Shaders;
using Drizzle.UI.Shared.Extensions;
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
        UpdateUniforms(texture.GraphicsDevice, timespan);
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
