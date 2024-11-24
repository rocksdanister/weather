﻿using ComputeSharp.D2D1.Uwp;
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

public sealed class SnowRunner : ID2D1ShaderRunner, IDisposable
{
    private readonly PixelShaderEffect<Snow>? pixelShaderEffect;
    private readonly Func<SnowModel> properties;
    private readonly SnowModel currentProperties;
    private float4 mouseOffset = float4.Zero;
    private double simulatedTime, previousTime;

    public SnowRunner()
    {
        this.properties ??= () => new SnowModel();
        this.currentProperties ??= new SnowModel();
        this.pixelShaderEffect = new PixelShaderEffect<Snow>()
        {
            ResourceTextureManagers =
            {
                [0] = ComputeSharpUtil.CreateD2D1ResourceTextureManagerOrPlaceholder(currentProperties.ImagePath)
            }
        };
    }

    public SnowRunner(Func<SnowModel> properties) : this()
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

        // Update textures
        if (currentProperties.ImagePath != properties().ImagePath)
            this.pixelShaderEffect.ResourceTextureManagers[0] = ComputeSharpUtil.CreateD2D1ResourceTextureManagerOrPlaceholder(properties().ImagePath);

        // Update uniforms
        UpdateUniforms(args.Timing.TotalTime);

        // Set the uniforms
        this.pixelShaderEffect.ConstantBuffer = new Snow((float)simulatedTime,
            new int2(widthInPixels, heightInPixels),
            mouseOffset,
            currentProperties.Speed,
            currentProperties.Depth,
            currentProperties.Width,
            currentProperties.Brightness,
            currentProperties.Saturation,
            currentProperties.Layers,
            currentProperties.PostProcessing,
            currentProperties.IsLightning,
            currentProperties.IsBlur);

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
