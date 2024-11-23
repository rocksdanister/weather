using ComputeSharp.D2D1.Interop;
using ComputeSharp.D2D1;
using ComputeSharp.D2D1.Uwp;
using Drizzle.UI.Shaders.D2D1;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using ComputeSharp;
using Windows.ApplicationModel;
using System.IO;
using Drizzle.Models.Shaders;
using Drizzle.Common.Helpers;
using Drizzle.UI.Shared.Extensions;

namespace Drizzle.UI.UWP.Shaders.D2D1.Runners;

public sealed class ProteinCloudsRunner : ID2D1ShaderRunner, IDisposable
{
    private PixelShaderEffect<Clouds>? pixelShaderEffect;
    private readonly Func<CloudsModel> properties;
    private readonly CloudsModel currentProperties;
    private float4 mouseOffset = float4.Zero;
    private double simulatedTime, previousTime;

    public ProteinCloudsRunner()
    {
        this.properties ??= () => new CloudsModel();
        this.currentProperties ??= new CloudsModel();
    }

    public ProteinCloudsRunner(Func<CloudsModel> properties) : this()
    {
        this.properties = properties;
        this.currentProperties = new(properties());
    }

    public unsafe void Execute(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
    {
        if (this.pixelShaderEffect is null)
        {
            // Create the new pixel shader effect
            this.pixelShaderEffect = new PixelShaderEffect<Clouds>();
        }

        int widthInPixels = sender.ConvertDipsToPixels((float)sender.Size.Width, CanvasDpiRounding.Round);
        int heightInPixels = sender.ConvertDipsToPixels((float)sender.Size.Height, CanvasDpiRounding.Round);

        UpdateUniforms(args.Timing.TotalTime);
        // Set the constant buffer
        this.pixelShaderEffect.ConstantBuffer = new Clouds((float)simulatedTime, 
            new int2(widthInPixels, heightInPixels),
            mouseOffset,
            currentProperties.Speed, currentProperties.Scale,
            currentProperties.Iterations,
            currentProperties.Brightness,
            currentProperties.Saturation,
            currentProperties.IsDaytime,
            currentProperties.IsDayNightShift);

        // Draw the shader
        args.DrawingSession.DrawImage(this.pixelShaderEffect);
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

    /// <inheritdoc/>
    public void Dispose()
    {
        this.pixelShaderEffect?.Dispose();
    }
}
