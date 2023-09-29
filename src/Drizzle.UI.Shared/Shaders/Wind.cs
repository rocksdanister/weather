using ComputeSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Devices.Printers;

namespace Drizzle.UI.Shared.Shaders;

/// <summary>
/// Wind of change 
/// Ported from <see href="https://www.shadertoy.com/view/ldBSDd"/>.
/// <para>Copyright 2014 Roman Bobniev (FatumR).</para>
/// <para>Licensed under the Apache License, Version 2.0 (the "License").</para>
/// </summary>
[AutoConstructor]
[EmbeddedBytecode(DispatchAxis.XY)]
public readonly partial struct Wind : IPixelShader<float4>
{
    private readonly float time;

    private readonly IReadOnlyNormalizedTexture2D<float4> imageTexture;

    private readonly IReadOnlyNormalizedTexture2D<float4> depthTexture;

    private readonly float4 mouse;

    private readonly float4 mouseOffset;

    private readonly float3 color1;

    private readonly float3 color2;

    private readonly float speed;

    private readonly float amplitude;

    private readonly float2 parallaxIntensity;

    private readonly float brightness;

    private readonly float saturation;

    float rand(float2 co)
    {
        return Hlsl.Frac(Hlsl.Sin(Hlsl.Dot(co.XY, new float2(12.9898f, 78.233f))) * 43758.5453f);
    }

    float rand2(float2 co)
    {
        return Hlsl.Frac(Hlsl.Cos(Hlsl.Dot(co.XY, new float2(12.9898f, 78.233f))) * 43758.5453f);
    }

    // Rough Value noise implementation
    float valueNoiseSimple(float2 vl)
    {
        float minStep = 1f;

        float2 grid = Hlsl.Floor(vl);
        float2 gridPnt1 = grid;
        float2 gridPnt2 = new float2(grid.X, grid.Y + minStep);
        float2 gridPnt3 = new float2(grid.X + minStep, grid.Y);
        float2 gridPnt4 = new float2(gridPnt3.X, gridPnt2.Y);

        float s = rand2(grid);
        float t = rand2(gridPnt3);
        float u = rand2(gridPnt2);
        float v = rand2(gridPnt4);

        float x1 = Hlsl.SmoothStep(0f, 1f, Hlsl.Frac(vl.X));
        float interpX1 = Hlsl.Lerp(s, t, x1);
        float interpX2 = Hlsl.Lerp(u, v, x1);

        float y = Hlsl.SmoothStep(0f, 1f, Hlsl.Frac(vl.Y));
        float interpY = Hlsl.Lerp(interpX1, interpX2, y);

        return interpY;
    }

    float fractalNoise(float2 vl)
    {
        float persistance = 2.0f;
        float a = amplitude;
        float rez = 0f;
        float2 p = vl;

        for (float i = 0f; i < 8; i++)
        {
            rez += a * valueNoiseSimple(p);
            a /= persistance;
            p *= persistance;
        }
        return rez;
    }

    float complexFBM(float2 p)
    {
        float sound = 0f;
        float slow = time / speed;
        float fast = time / (speed/5f);
        float2 offset1 = new float2(slow, 0f); // Main front
        float2 offset2 = new float2(Hlsl.Sin(fast) * 0.1f, 0f); // sub fronts

        return (1f + sound) * fractalNoise(p + offset1 + fractalNoise(
                    p + fractalNoise(
                        p + 2f * fractalNoise(p - offset2)
                    )
                )
            );
    }

    //ref: https://stackoverflow.com/questions/9320953/what-algorithm-does-photoshop-use-to-desaturate-an-image
    float4 Desaturate(float4 color)
    {
        float bw = (Hlsl.Min(color.R, Hlsl.Min(color.G, color.B)) + Hlsl.Min(color.R, Hlsl.Min(color.G, color.B))) * 0.5f;
        return new float4(bw, bw, bw, 1f);
    }

    /// <inheritdoc/>
    public float4 Execute()
    {
        float2 fragCoord = new(ThreadIds.X, DispatchSize.Y - ThreadIds.Y);
        float2 uv = fragCoord / DispatchSize.XY;
        uv.Y = 1.0f - uv.Y;

        // Fill scale
        float screenAspect = (float)DispatchSize.X / DispatchSize.Y;
        float textureAspect = (float)imageTexture.Width / imageTexture.Height;
        float scaleX = 1f, scaleY = 1f;
        if (textureAspect > screenAspect)
            scaleX = screenAspect / textureAspect;
        else
            scaleY = textureAspect / screenAspect;
        uv = new Float2(scaleX, scaleY) * (uv - 0.5f) + 0.5f;

        float depth = depthTexture.Sample(uv).R;
        float2 parallax = mouseOffset.XY * depth * parallaxIntensity;

        float3 rez = Hlsl.Lerp(color2, color1, complexFBM(uv));
        float4 color = new float4(rez + imageTexture.Sample(uv + parallax).RGB, 1f);

        color = saturation < 1f ? Hlsl.Lerp(Desaturate(color), color, saturation) : color;
        return color * brightness * 0.75f;
    }
}

