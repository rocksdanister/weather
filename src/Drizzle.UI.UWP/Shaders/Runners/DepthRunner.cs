using ComputeSharp;
using ComputeSharp.Uwp;
using Drizzle.Common.Helpers;
using Drizzle.Models.Shaders;
using Drizzle.UI.Shared.Extensions;
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
        UpdateUniforms(texture.GraphicsDevice);
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

    private void UpdateUniforms(GraphicsDevice device)
    {
        // Textures
        if (currentProperties.ImagePath != properties().ImagePath
            || currentProperties.DepthPath != properties().DepthPath
            || this.image is null
            || this.depth is null
            || this.image.GraphicsDevice != device
            || this.depth.GraphicsDevice != device)
        {
            this.image?.Dispose();
            this.depth?.Dispose();

            this.image = ComputeSharpUtil.CreateTextureOrPlaceholder(properties().ImagePath, device);
            this.depth = ComputeSharpUtil.CreateTextureOrPlaceholder(properties().DepthPath, device);
        }

        // Mouse
        currentProperties.Mouse = properties().Mouse;
        currentProperties.MouseSpeed = properties().MouseSpeed;
        currentProperties.MouseInertia = properties().MouseInertia;
        mouseOffset.X += (currentProperties.MouseSpeed * currentProperties.Mouse.X - mouseOffset.X) * currentProperties.MouseInertia;
        mouseOffset.Y += (currentProperties.MouseSpeed * currentProperties.Mouse.Y - mouseOffset.Y) * currentProperties.MouseInertia;

        // Time
        currentProperties.TimeMultiplier = MathUtil.Lerp(currentProperties.TimeMultiplier, properties().TimeMultiplier, 0.05f);

        // Shader specific
        properties().UniformFrameUpdate(currentProperties);
    }
}
