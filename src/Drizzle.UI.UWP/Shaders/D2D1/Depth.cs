﻿using ComputeSharp;
using ComputeSharp.D2D1;

namespace Drizzle.UI.Shaders.D2D1;

/// <summary>
/// Parallax depth effect using depthmaps
/// <para>Created by Dani John (https://github.com/rocksdanister).</para>
/// <para>License MIT.</para>
/// </summary>
[D2DInputCount(0)]
[D2DRequiresScenePosition]
[D2DShaderProfile(D2D1ShaderProfile.PixelShader50)]
[AutoConstructor]
public readonly partial struct Depth : ID2D1PixelShader
{
    private readonly int2 dispatchSize;

    private readonly float4 mouse;

    private readonly float4 mouseOffset;

    private readonly float2 intensity;

    private readonly bool isBlur;

    private readonly float saturation;

    private readonly float brightness;

    [D2DResourceTextureIndex(0)]
    private readonly D2D1ResourceTexture2D<float4> imageTexture;

    [D2DResourceTextureIndex(1)]
    private readonly D2D1ResourceTexture2D<float4> depthTexture;

    //ref: https://stackoverflow.com/questions/9320953/what-algorithm-does-photoshop-use-to-desaturate-an-image
    float4 Desaturate(float4 color)
    {
        float bw = (Hlsl.Min(color.R, Hlsl.Min(color.G, color.B)) + Hlsl.Min(color.R, Hlsl.Min(color.G, color.B))) * 0.5f;
        return new float4(bw, bw, bw, 1f);
    }

    /// <inheritdoc/>
    public float4 Execute()
    {
        float2 fragCoord = new(D2D.GetScenePosition().X, dispatchSize.Y - D2D.GetScenePosition().Y);
        float2 uv = fragCoord / dispatchSize.XY;
        uv.Y = 1.0f - uv.Y;

        // Fill scale
        float screenAspect = (float)dispatchSize.X / dispatchSize.Y;
        float textureAspect = (float)imageTexture.Width / imageTexture.Height;
        float scaleX = 1f, scaleY = 1f;
        if (textureAspect > screenAspect)
            scaleX = screenAspect / textureAspect;
        else
            scaleY = textureAspect / screenAspect;
        uv = new Float2(scaleX, scaleY) * (uv - 0.5f) + 0.5f;

        float depth = depthTexture.Sample(uv).R;
        float2 parallax = mouseOffset.XY * depth * intensity;

        float4 color = float4.Zero;

        if (isBlur)
        {
            float3x3 kernel = new float3x3(1f, 2f, 1f, 2f, 4f, 2f, 1f, 2f, 1f) * 0.0625f;
            float1x3 direction = new float1x3(-1.0f, 0.0f, 1.0f);
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    float2 offset = new float2(direction[0][x], direction[0][y]) / dispatchSize.XY;
                    color += imageTexture.Sample(uv + parallax + offset) * kernel[x][y];
                }
            }
        }
        else
        {
            color = new(imageTexture.Sample(uv + parallax).RGB, 1);
        }

        color = saturation < 1f ? Hlsl.Lerp(Desaturate(color), color, saturation) : color;
        return color * brightness;
    }
}