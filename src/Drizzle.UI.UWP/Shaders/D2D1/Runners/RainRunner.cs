﻿using ComputeSharp.D2D1.Uwp;
using Drizzle.Common.Helpers;
using Drizzle.Models.Shaders;
using Drizzle.UI.Shaders.D2D1;
using Drizzle.UI.Shared.Extensions;
using Drizzle.UI.UWP.Helpers;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using Windows.Foundation;

namespace Drizzle.UI.UWP.Shaders.D2D1.Runners;

public sealed class RainRunner : ID2D1ShaderRunner, IDisposable
{
    private readonly PixelShaderEffect<Rain>? pixelShaderEffect;
    private readonly Func<RainModel> properties;
    private readonly RainModel currentProperties;
    private readonly BorderEffect borderEffect;
    private float4 mouseOffset = float4.Zero;
    private double simulatedTime, previousTime;
    private CanvasBitmap image;

    public RainRunner()
    {
        this.properties ??= () => new RainModel();
        this.currentProperties ??= new RainModel();
        this.pixelShaderEffect = new PixelShaderEffect<Rain>();
        // Droplets at borders will reflect this.
        this.borderEffect = new BorderEffect
        {
            ExtendX = CanvasEdgeBehavior.Clamp,
            ExtendY = CanvasEdgeBehavior.Clamp
        };
    }

    public RainRunner(Func<RainModel> properties) : this()
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
            || image.Dpi != sender.Dpi
            || image.Device != sender.Device
            || currentProperties.ImagePath != properties().ImagePath)
        {
            borderEffect.Source = null;
            image?.Dispose();
            image = ComputeSharpUtil.CreateCanvasBitmapOrPlaceholder(sender, properties().ImagePath, sender.Dpi);
            borderEffect.Source = image;

            this.pixelShaderEffect.Sources[0] = borderEffect;
        }

        // Update uniforms
        UpdateUniforms(args.Timing.TotalTime);

        // Set the constant buffer
        this.pixelShaderEffect.ConstantBuffer = new Rain((float)simulatedTime,
            new int2(widthInPixels, heightInPixels),
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
