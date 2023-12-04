using ComputeSharp;
using System;
using Drizzle.UI.Shared.Shaders.Models;
using Drizzle.UI.Shared.Shaders.Helpers;
#if WINDOWS_UWP
using ComputeSharp.Uwp;
#else
using ComputeSharp.WinUI;
#endif

namespace Drizzle.UI.Shared.Shaders.Runners;

public sealed class CloudsRunner : IShaderRunner
{
    private readonly Func<CloudsModel> properties;
    private readonly CloudsModel currentProperties;
    private float4 mouseOffset = float4.Zero;
    private double simulatedTime, previousTime;
    private float currentTimeStep = 0.75f;

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
        UpdateProperties();
        // Adjust delta instead of actual time/speed to avoid rewinding time
        simulatedTime += (timespan.TotalSeconds - previousTime) * currentProperties.TimeMultiplier;
        previousTime = timespan.TotalSeconds;

        texture.GraphicsDevice.ForEach(texture, new Clouds((float)simulatedTime,
            mouseOffset,
            currentProperties.Speed,
            currentProperties.Scale,
            currentProperties.Iterations,
            currentProperties.Brightness,
            currentProperties.Saturation,
            currentTimeStep, 
            currentProperties.IsDayNightShift));

        return true;
    }

    private void UpdateProperties()
    {
        // Smoothing
        currentProperties.Brightness = ShaderUtil.Lerp(currentProperties.Brightness, properties().Brightness, 0.05f);
        currentProperties.Saturation = ShaderUtil.Lerp(currentProperties.Saturation, properties().Saturation, 0.01f);
        currentProperties.TimeMultiplier = ShaderUtil.Lerp(currentProperties.TimeMultiplier, properties().TimeMultiplier, 0.05f);
        currentTimeStep = ShaderUtil.Lerp(currentTimeStep, currentProperties.IsDaytime ? 0.75f : 0.25f, 0.025f);
        // Mouse
        currentProperties.Mouse = properties().Mouse;
        mouseOffset.X += (1.5f * properties().Mouse.X - mouseOffset.X) * 0.08f;
        mouseOffset.Y += (1.5f * properties().Mouse.Y - mouseOffset.Y) * 0.08f;
        // Other
        currentProperties.Scale = properties().Scale;
        currentProperties.Iterations = properties().Iterations;
        currentProperties.Speed = properties().Speed;
        currentProperties.IsDayNightShift = properties().IsDayNightShift;
        currentProperties.IsDaytime = properties().IsDaytime;
    }

    /// <summary>
    /// Returns 0 and 0.5 when the time is between 6 PM and 6 AM and value between 0.5 to 1 when the time is between 6 AM and 6 PM.<br>
    /// In the shader timeStep < 0.5 is blue and > 0.5 is orange.</br>
    /// </summary>
    private static float GetTimeStep(DateTime time)
    {
        int hour = time.Hour;
        if (hour >= 6 && hour < 18)
            return (hour - 6) / 24f + 0.5f;
        else if (hour >= 18 && hour <= 23)
            return (hour - 18) / 24f;
        else
            return hour / 24f + 0.25f;
    }
}
