using ComputeSharp;
using System;
using ComputeSharp.Uwp;
using Drizzle.Models.Shaders;
using Drizzle.Common.Helpers;
using Drizzle.Models.Shaders.Uniform;
using Drizzle.UI.Shared.Extensions;

namespace Drizzle.UI.Shaders.Runners;

public sealed class CloudsRunner : IShaderRunner
{
    private readonly Func<CloudsModel> properties;
    private readonly CloudsModel currentProperties;
    private float4 mouseOffset = float4.Zero;
    private double simulatedTime, previousTime;

    public CloudsRunner()
    {
        this.properties ??= () => new CloudsModel();
        this.currentProperties ??= new CloudsModel();
    }

    public CloudsRunner(Func<CloudsModel> properties) : this()
    {
        this.properties = properties;
        this.currentProperties = new(properties());
    }

    public bool TryExecute(IReadWriteNormalizedTexture2D<float4> texture, TimeSpan timespan, object parameter)
    {
        UpdateUniforms(timespan);
        texture.GraphicsDevice.ForEach(texture, new Clouds((float)simulatedTime,
            mouseOffset,
            currentProperties.Speed,
            currentProperties.Scale,
            currentProperties.Iterations,
            currentProperties.Brightness,
            currentProperties.Saturation,
            currentProperties.IsDaytime, 
            currentProperties.IsDayNightShift));

        return true;
    }

    private void UpdateUniforms(TimeSpan timespan)
    {
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
