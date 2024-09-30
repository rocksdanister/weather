using ComputeSharp;
using ComputeSharp.Uwp;
using Drizzle.Common.Helpers;
using Drizzle.Models.Shaders;
using Drizzle.UI.UWP.Helpers;
using System;

namespace Drizzle.UI.Shaders.Runners;

public sealed class TunnelRunner : IShaderRunner
{
    private readonly Func<TunnelModel> properties;
    private readonly TunnelModel currentProperties;
    private ReadOnlyTexture2D<Rgba32, float4> image;
    private double simulatedTime, previousTime;

    public TunnelRunner()
    {
        this.properties ??= () => new TunnelModel();
        this.currentProperties ??= new TunnelModel();
        image = ComputeSharpUtil.CreateTextureOrPlaceholder(currentProperties.ImagePath, GraphicsDevice.GetDefault());
    }

    public TunnelRunner(Func<TunnelModel> properties) : this()
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

        texture.GraphicsDevice.ForEach(texture, new Tunnel((float)simulatedTime, image, currentProperties.Brightness, currentProperties.Speed, currentProperties.IsSquare));

        return true;

    }

    private void UpdateProperties()
    {
        // Smoothing, value is increased by small step % every frame
        currentProperties.TimeMultiplier = MathUtil.Lerp(currentProperties.TimeMultiplier, properties().TimeMultiplier, 0.05f);
        currentProperties.Brightness = MathUtil.Lerp(currentProperties.Brightness, properties().Brightness, 0.05f);
        // Other
        currentProperties.IsSquare = properties().IsSquare;
        currentProperties.Speed = properties().Speed;
    }
}
