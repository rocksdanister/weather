using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using Drizzle.Common.Helpers;
using Drizzle.Models.Enums;
using Drizzle.Models.Shaders;
using Drizzle.Models.Shaders.Uniform;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace Drizzle.UI.Avalonia.UserControls.Shaders;

internal class DrawCompositionCustomVisualHandler : CompositionCustomVisualHandler
{
    private bool isRunning;
    private Stretch? stretch;
    private StretchDirection? stretchDirection;
    private Size? size;
    private readonly object sync = new();
    private SKPaint? paint;
    private SKShader? shader;
    private SKRuntimeEffectUniforms? uniforms;
    private SKRuntimeEffect? effect;
    private SKRuntimeEffectChildren? children;
    private float time;
    private bool isDisposed;
    private Vector4 mouseOffset = Vector4.Zero;
    // To track uniforms for lerp animation
    private readonly Dictionary<string, float> uniformFloats = [];
    // To keep track of texture change
    private readonly Dictionary<string, (string? assetPath, SKShader? imageShader, Vector3 dimensions)> uniformTextures = [];
    private ShaderModel? shaderModel;
    // ShaderModel defaults
    private float simulatedTime = 0;
    private float previousTime = 0;
    private float timeMultiplier = 1;
    private float brightness = 1;
    private float saturation = 1;

    public override void OnMessage(object message)
    {
        if (message is not DrawPayload msg)
            return;

        switch (msg)
        {
            case
            {
                HandlerCommand: HandlerCommand.Start,
                Model: { } model,
                Size: { } currentSize,
                Stretch: { } st,
                StretchDirection: { } sd
            }:
                {
                    shaderModel = model;
                    isDisposed = false;
                    CreatePaint();

                    isRunning = true;
                    this.size = currentSize;
                    stretch = st;
                    stretchDirection = sd;
                    RegisterForNextAnimationFrameUpdate();
                    break;
                }
            case
            {
                HandlerCommand: HandlerCommand.Start
            }:
                {
                    isRunning = true;
                    RegisterForNextAnimationFrameUpdate();
                    break;
                }
            case
            {
                HandlerCommand: HandlerCommand.Update,
                Size: { } currentSize,
                Stretch: { } st,
                StretchDirection: { } sd
            }:
                {
                    this.size = currentSize;
                    stretch = st;
                    stretchDirection = sd;
                    RegisterForNextAnimationFrameUpdate();
                    break;
                }
            case
            {
                HandlerCommand: HandlerCommand.Pause
            }:
                {
                    isRunning = false;
                    break;
                }
            case
            {
                HandlerCommand: HandlerCommand.Stop
            }:
                {
                    ResetTime();
                    isRunning = false;
                    break;
                }
            case
            {
                HandlerCommand: HandlerCommand.Dispose
            }:
                {
                    DisposeImpl();
                    break;
                }
        }
    }

    private void ResetTime()
    {
        time = 0;
        simulatedTime = 0;
        previousTime = 0;
        timeMultiplier = 0;
    }

    public override void OnAnimationFrameUpdate()
    {
        if (!isRunning || isDisposed)
            return;

        Invalidate();
        RegisterForNextAnimationFrameUpdate();
    }

    private void DisposeImpl()
    {
        lock (sync)
        {
            if (isDisposed)
                return;

            isRunning = false;
            isDisposed = true;
            effect?.Dispose();
            uniforms?.Reset();
        }
    }

    private void CreatePaint()
    {
        if (shaderModel is null)
            return;

        // Initialize uniform temp fields.
        uniformFloats.Clear();
        uniformTextures.Clear();
        foreach (var item in shaderModel.UniformMappings)
        {
            if (item.Value.UniformType == UniformTypes.float_ && ((FloatProperty)item.Value).LerpSpeed != 0)
                uniformFloats.Add(item.Value.UniformName, (float)item.Value.GetValue(shaderModel));
            else if (item.Value.UniformType == UniformTypes.textureUri)
                uniformTextures.Add(item.Value.UniformName, (null, null, Vector3.Zero));
        }

        time = 0f;

        var rb = GetRenderBounds();
        var currentSize = this.size ?? rb.Size;

        var uri = shaderModel.ShaderUri;
        using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);
        var shaderCode = reader.ReadToEnd();

        effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
        if (errorText != null)
            Debug.WriteLine(errorText);

        uniforms = new SKRuntimeEffectUniforms(effect)
        {
            ["u_Resolution"] = new[] { (float)currentSize.Width, (float)currentSize.Height, 0f },
            ["u_Time"] = time,
            ["u_Mouse"] = new[] { 0f, 0f, -1f, -1f },
            //["u_TextureResolution"] = new[] { 1620f, 1080f, 0f },
        };

        //ref: https://skia.org/docs/user/sksl/
        children = new SKRuntimeEffectChildren(effect);

        shader = effect.ToShader(uniforms, children);
        paint = new SKPaint { Shader = shader };
    }

    private void UpdatePaint()
    {
        if (isRunning && uniforms is { } && effect is { } && paint is { })
        {
            // Time (delta)
            time += 1f / 60f;
            timeMultiplier = MathUtil.Lerp(timeMultiplier, shaderModel.TimeMultiplier, 0.05f);
            simulatedTime += (time - previousTime) * timeMultiplier;
            previousTime = time;
            uniforms["u_Time"] = simulatedTime;
            // ShaderModel defaults
            brightness = MathUtil.Lerp(brightness, shaderModel.Brightness, 0.02f);
            uniforms["u_Brightness"] = brightness;
            saturation = MathUtil.Lerp(saturation, shaderModel.Saturation, 0.01f);
            uniforms["u_Saturation"] = saturation;
            // Mouse
            mouseOffset.X += (shaderModel.MouseSpeed * shaderModel.Mouse.X - mouseOffset.X) * shaderModel.MouseInertia;
            mouseOffset.Y += (shaderModel.MouseSpeed * shaderModel.Mouse.Y - mouseOffset.Y) * shaderModel.MouseInertia;
            uniforms["u_Mouse"] = new[] { mouseOffset.X, mouseOffset.Y, 0f, 0f };

            // Update custom uniforms
            foreach (var item in shaderModel.UniformMappings)
            {
                switch (item.Value.UniformType)
                {
                    case UniformTypes.bool_:
                        {
                            uniforms[item.Value.UniformName] = (bool)(item.Value.GetValue(shaderModel)) ? 1 : 0;
                        }
                        break;
                    case UniformTypes.int_:
                        {
                            uniforms[item.Value.UniformName] = (int)item.Value.GetValue(shaderModel);
                        }
                        break;
                    case UniformTypes.float_:
                        {
                            var property = item.Value as FloatProperty;
                            if (property.LerpSpeed != 0)
                            { 
                                uniformFloats[item.Value.UniformName] = MathUtil.Lerp(uniformFloats[property.UniformName], (float)property.GetValue(shaderModel), property.LerpSpeed);
                                uniforms[item.Value.UniformName] = uniformFloats[item.Value.UniformName];
                            }
                            else
                            {
                                uniforms[property.UniformName] = (float)property.GetValue(shaderModel);
                            }
                        }
                        break;
                    case UniformTypes.color:
                        {
                            var color = (Vector3)item.Value.GetValue(shaderModel);
                            uniforms[item.Value.UniformName] = new[] { color.X, color.Y, color.Z };
                        }
                        break;
                    case UniformTypes.textureUri:
                        {
                            var property = item.Value as TextureProperty;
                            var assetPath = (string)item.Value.GetValue(shaderModel);
                            var currentValue = uniformTextures[item.Value.UniformName];
                            if (assetPath != currentValue.assetPath)
                            {
                                var (imageShader, dimensions) = CreateImageShader(new Uri(assetPath), WrapToSkTile(property.WrapMode));
                                currentValue = (assetPath, imageShader, dimensions);
                                uniformTextures[item.Value.UniformName] = currentValue;
                            }
                            uniforms[$"{item.Value.UniformName}Resolution"] = new[] { currentValue.dimensions.X, currentValue.dimensions.Y, 0 };
                            children ??= new SKRuntimeEffectChildren(effect);
                            children[item.Value.UniformName] = currentValue.imageShader;
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }


            shader?.Dispose();
            shader = effect.ToShader(uniforms, children);
            paint.Shader = shader;
        }
    }

    private void Draw(SKCanvas canvas)
    {
        if (isDisposed || effect is null)
            return;

        canvas.Save();

        var rb = GetRenderBounds();
        var currentSize = size ?? rb.Size;
        //var currentSize = (Size)shaderSize;

        if (paint is null)
        {
            CreatePaint();
            if (paint is null)
                return;
        }
        else
        {
            uniforms = new SKRuntimeEffectUniforms(effect)
            {
                ["u_Resolution"] = new[] { (float)currentSize.Width, (float)currentSize.Height, 0f },
            };
            UpdatePaint();
        }

        canvas.DrawRect(0, 0, (float)currentSize.Width, (float)currentSize.Height, new SKPaint { Color = SKColors.Black });
        canvas.DrawRect(0, 0, (float)currentSize.Width, (float)currentSize.Height, paint);

        canvas.Restore();
    }

    public override void OnRender(ImmediateDrawingContext context)
    {
        lock (sync)
        {
            if (stretch is not { } st
                || stretchDirection is not { } sd)
            {
                return;
            }

            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null)
            {
                return;
            }

            var rb = GetRenderBounds();

            var currentSize = this.size ?? rb.Size;

            var viewPort = new Rect(rb.Size);
            var sourceSize = new Size(currentSize.Width, currentSize.Height);
            if (sourceSize.Width <= 0 || sourceSize.Height <= 0)
                return;

            var scale = st.CalculateScaling(rb.Size, sourceSize, sd);
            var scaledSize = sourceSize * scale;
            var destRect = viewPort
                .CenterRect(new Rect(scaledSize))
                .Intersect(viewPort);
            var sourceRect = new Rect(sourceSize)
                .CenterRect(new Rect(destRect.Size / scale));

            var bounds = SKRect.Create(new SKPoint(), new SKSize((float)currentSize.Width, (float)currentSize.Height));
            var scaleMatrix = Matrix.CreateScale(
                destRect.Width / sourceRect.Width,
                destRect.Height / sourceRect.Height);
            var translateMatrix = Matrix.CreateTranslation(
                -sourceRect.X + destRect.X - bounds.Top,
                -sourceRect.Y + destRect.Y - bounds.Left);

            using (context.PushClip(destRect))
            using (context.PushPostTransform(translateMatrix * scaleMatrix))
            {
                using var lease = leaseFeature.Lease();
                var canvas = lease?.SkCanvas;
                if (canvas is null)
                {
                    return;
                }
                Draw(canvas);
            }
        }
    }

    private static (SKShader? imageShader, Vector3 dimensions) CreateImageShader(Uri assetUri, SKShaderTileMode wrap = SKShaderTileMode.Clamp)
    {
        using var image = assetUri != null ? SKImage.FromEncodedData(AssetLoader.Open(assetUri)) : null;
        var dimensions = image != null ? new Vector3(image.Width, image.Height, 0) : Vector3.Zero;
        return new(image?.ToShader(wrap, wrap), dimensions);
    }

    private static SKShaderTileMode WrapToSkTile(TextureWrapMode wrap)
    {
        return wrap switch
        {
            TextureWrapMode.clamp => SKShaderTileMode.Clamp,
            TextureWrapMode.repeat => SKShaderTileMode.Repeat,
            TextureWrapMode.mirror => SKShaderTileMode.Mirror,
            _ => SKShaderTileMode.Clamp,
        };
    }
}
