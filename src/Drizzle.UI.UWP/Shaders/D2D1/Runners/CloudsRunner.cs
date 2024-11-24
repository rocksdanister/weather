using ComputeSharp.D2D1.Uwp;
using Drizzle.Common.Helpers;
using Drizzle.Models.Shaders;
using Drizzle.UI.Shaders.D2D1;
using Drizzle.UI.Shared.Extensions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.Foundation;
using System;

namespace Drizzle.UI.UWP.Shaders.D2D1.Runners;

public sealed class CloudsRunner : ID2D1ShaderRunner, IDisposable
{
    private readonly PixelShaderEffect<Clouds>? pixelShaderEffect;
    private readonly Func<CloudsModel> properties;
    private readonly CloudsModel currentProperties;
    private float4 mouseOffset = float4.Zero;
    private double simulatedTime, previousTime;

    public CloudsRunner()
    {
        this.pixelShaderEffect = new PixelShaderEffect<Clouds>();
        this.properties ??= () => new CloudsModel();
        this.currentProperties ??= new CloudsModel();
    }

    public CloudsRunner(Func<CloudsModel> properties) : this()
    {
        this.properties = properties;
        this.currentProperties = new(properties());
    }

    public unsafe void Execute(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args, double resolutionScale)
    {
        var canvasSize = sender.Size;
        var renderSize = new Size(canvasSize.Width * resolutionScale, canvasSize.Height * resolutionScale);

        int widthInPixels = sender.ConvertDipsToPixels((float)renderSize.Width, CanvasDpiRounding.Round);
        int heightInPixels = sender.ConvertDipsToPixels((float)renderSize.Height, CanvasDpiRounding.Round);

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
        args.DrawingSession.DrawImage(image: this.pixelShaderEffect, 
            destinationRectangle: new Rect(0, 0, canvasSize.Width, canvasSize.Height),
            sourceRectangle: new Rect(0, 0, renderSize.Width, renderSize.Height));
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
