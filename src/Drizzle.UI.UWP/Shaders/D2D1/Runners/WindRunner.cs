using ComputeSharp.D2D1.Uwp;
using Drizzle.Common.Helpers;
using Drizzle.Models.Shaders;
using Drizzle.UI.Shaders.D2D1;
using Drizzle.UI.Shared.Extensions;
using Drizzle.UI.UWP.Helpers;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using Windows.Foundation;

namespace Drizzle.UI.UWP.Shaders.D2D1.Runners;

public sealed class WindRunner : ID2D1ShaderRunner, IDisposable
{
    private readonly PixelShaderEffect<Wind>? pixelShaderEffect;
    private readonly Func<WindModel> properties;
    private readonly WindModel currentProperties;
    private float4 mouseOffset = float4.Zero;
    private double simulatedTime, previousTime;
    private CanvasBitmap image, depth;

    public WindRunner()
    {
        this.properties ??= () => new WindModel();
        this.currentProperties ??= new WindModel();
        this.pixelShaderEffect = new PixelShaderEffect<Wind>();

    }

    public WindRunner(Func<WindModel> properties) : this()
    {
        this.properties = properties;
        this.currentProperties = new(properties());
    }

    public void Execute(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args, double resolutionScale)
    {
        var canvasSize = sender.Size;
        var renderSize = new Size(canvasSize.Width * resolutionScale, canvasSize.Height * resolutionScale);

        int widthInPixels = sender.ConvertDipsToPixels((float)renderSize.Width, CanvasDpiRounding.Round);
        int heightInPixels = sender.ConvertDipsToPixels((float)renderSize.Height, CanvasDpiRounding.Round);

        // Update textures
        if (image == null
            || depth == null
            || image.Dpi != sender.Dpi
            || depth.Dpi != sender.Dpi
            || image.Device != sender.Device
            || depth.Device != sender.Device
            || currentProperties.ImagePath != properties().ImagePath
            || currentProperties.DepthPath != properties().DepthPath)
        {
            image?.Dispose();
            depth?.Dispose();
            image = ComputeSharpUtil.CreateCanvasBitmapOrPlaceholder(sender, properties().ImagePath, sender.Dpi);
            depth = ComputeSharpUtil.CreateCanvasBitmapOrPlaceholder(sender, properties().DepthPath, sender.Dpi);

            this.pixelShaderEffect.Sources[0] = image;
            this.pixelShaderEffect.Sources[1] = depth;
        }

        // Update uniforms
        UpdateUniforms(args.Timing.TotalTime);

        // Set the uniforms
        this.pixelShaderEffect.ConstantBuffer = new Wind((float)simulatedTime,
            new int2(widthInPixels, heightInPixels),
            new float4(currentProperties.Mouse.X, currentProperties.Mouse.Y, currentProperties.Mouse.W, currentProperties.Mouse.Z),
            mouseOffset,
            new float3(currentProperties.Color1.X, currentProperties.Color1.Y, currentProperties.Color1.Z),
            new float3(currentProperties.Color2.X, currentProperties.Color2.Y, currentProperties.Color2.Z),
            currentProperties.MaxSpeed - currentProperties.Speed + currentProperties.MinSpeed,
            currentProperties.Amplitude,
            new float2(currentProperties.ParallaxIntensityX, currentProperties.ParallaxIntensityY),
            currentProperties.Brightness,
            currentProperties.Saturation,
            new int2((int)image.Size.Width, (int)image.Size.Height));

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