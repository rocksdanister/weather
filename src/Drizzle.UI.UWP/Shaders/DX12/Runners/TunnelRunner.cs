using ComputeSharp;
using ComputeSharp.Uwp;
using Drizzle.Common.Helpers;
using Drizzle.Models.Shaders;
using Drizzle.UI.Shared.Extensions;
using Drizzle.UI.UWP.Helpers;
using System;

namespace Drizzle.UI.Shaders.DX12.Runners;

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
        UpdateUniforms(texture.GraphicsDevice, timespan);
        texture.GraphicsDevice.ForEach(texture, new Tunnel((float)simulatedTime, image, currentProperties.Brightness, currentProperties.Speed, currentProperties.IsSquare));

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

        // Time, adjust delta instead of actual time/speed to avoid rewinding time
        currentProperties.TimeMultiplier = MathUtil.Lerp(currentProperties.TimeMultiplier, properties().TimeMultiplier, 0.05f);
        simulatedTime += (timespan.TotalSeconds - previousTime) * currentProperties.TimeMultiplier;
        previousTime = timespan.TotalSeconds;

        // Shader specific
        properties().UniformFrameUpdate(currentProperties);
    }
}
