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

public sealed class DepthRunner : ID2D1ShaderRunner, IDisposable
{
    private readonly PixelShaderEffect<Depth>? pixelShaderEffect;
    private readonly Func<DepthModel> properties;
    private readonly DepthModel currentProperties;
    private float4 mouseOffset = float4.Zero;

    public DepthRunner()
    {
        this.properties ??= () => new DepthModel();
        this.currentProperties ??= new DepthModel();
        this.pixelShaderEffect = new PixelShaderEffect<Depth>()
        {
            ResourceTextureManagers =
            {
                [0] = ComputeSharpUtil.CreateD2D1ResourceTextureManagerOrPlaceholder(currentProperties.ImagePath),
                [1] = ComputeSharpUtil.CreateD2D1ResourceTextureManagerOrPlaceholder(currentProperties.DepthPath)
            }
        };
    }

    public DepthRunner(Func<DepthModel> properties) : this()
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
        if (currentProperties.ImagePath != properties().ImagePath || currentProperties.DepthPath != properties().DepthPath)
        {
            this.pixelShaderEffect.ResourceTextureManagers[0] = ComputeSharpUtil.CreateD2D1ResourceTextureManagerOrPlaceholder(properties().ImagePath);
            this.pixelShaderEffect.ResourceTextureManagers[1] = ComputeSharpUtil.CreateD2D1ResourceTextureManagerOrPlaceholder(properties().DepthPath);
        }

        // Update uniforms
        UpdateUniforms();

        // Set the uniforms
        this.pixelShaderEffect.ConstantBuffer = new Depth(
            new int2(widthInPixels, heightInPixels),
            new float4(currentProperties.Mouse.X, currentProperties.Mouse.Y, currentProperties.Mouse.W, currentProperties.Mouse.Z),
            mouseOffset,
            new float2(currentProperties.IntensityX, currentProperties.IntensityY),
            currentProperties.IsBlur,
            currentProperties.Saturation,
            currentProperties.Brightness);

        // Draw the shader
        args.DrawingSession.DrawImage(image: this.pixelShaderEffect,
            destinationRectangle: new Rect(0, 0, canvasSize.Width, canvasSize.Height),
            sourceRectangle: new Rect(0, 0, renderSize.Width, renderSize.Height));
    }

    private void UpdateUniforms()
    {
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

    /// <inheritdoc/>
    public void Dispose()
    {
        this.pixelShaderEffect?.Dispose();
    }
}
