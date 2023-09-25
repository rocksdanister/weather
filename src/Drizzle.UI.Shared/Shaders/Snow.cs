using ComputeSharp;

namespace Drizzle.UI.Shared.Shaders;

/// <summary>
/// Simple (but not cheap) snow made from multiple parallax layers with randomly positioned flakes and directions.
/// Ported from <see href="https://www.shadertoy.com/view/ldsGDn"/>.
/// <para>Copyright (c) 2013 Andrew Baldwin (twitter: baldand, www: http://thndl.com).</para>
/// <para>License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.</para>
/// </summary>
[AutoConstructor]
[EmbeddedBytecode(DispatchAxis.XY)]
public readonly partial struct Snow : IPixelShader<float4>
{
    private readonly float time;

    private readonly float4 mouse;

    private readonly float speed;

    private readonly float depth;

    private readonly float width;

    private readonly float brightness;

    private readonly float saturation;

    private readonly int layers;

    private readonly float postProcessing;

    private readonly bool isLightning;

    private readonly bool isBlur;

    private readonly IReadOnlyNormalizedTexture2D<float4> texture;

    // glsl style mod
    float Mod(float x, float y)
    {
        return (x - y * Hlsl.Floor(x / y));
    }

    float2 Mod(float2 n1, float2 n2)
    {
        return new float2(Mod(n1.X, n2.X), Mod(n1.Y, n2.Y));
    }

    float Rand(float2 co)
    {
        return Hlsl.Frac(Hlsl.Sin(Hlsl.Dot(co, new float2(12.9898f, 78.233f))) * 43758.5453f);
    }

    float S(float a, float b, float t)
    {
        return Hlsl.SmoothStep(a, b, t);
    }

    //ref: https://stackoverflow.com/questions/9320953/what-algorithm-does-photoshop-use-to-desaturate-an-image
    float3 Desaturate(float3 color)
    {
        float bw = (Hlsl.Min(color.R, Hlsl.Min(color.G, color.B)) + Hlsl.Min(color.R, Hlsl.Min(color.G, color.B))) * 0.5f;
        return new float3(bw, bw, bw);
    }

    public float4 Execute()
    {
        float2 fragCoord = new(ThreadIds.X, DispatchSize.Y - ThreadIds.Y);
        float3x3 p = new float3x3(13.323122f, 23.5112f, 21.71123f, 21.1212f, 28.7312f, 11.9312f, 21.8112f, 14.7212f, 61.3934f);
        float2 uv = mouse.XY + new float2(1f, (float)DispatchSize.Y / DispatchSize.X) * fragCoord.XY / DispatchSize.XY;// + u_mouse.xy / u_resolution.xy;
        float2 UV = fragCoord.XY / DispatchSize.XY;
        UV.Y = 1f - UV.Y;
        float T = time * speed;
        float3 acc = new float3(0f, 0f, 0f);
        float dof = 5f * Hlsl.Sin(T * .1f);
        for (int i = 0; i < layers; i++)
        {
            float fi = (float)i;
            float2 q = uv * (1f + fi * depth);
            q += new float2(q.Y * (width * Mod(fi * 7.238917f, 1f) - width * .5f), T / (1f + fi * depth * .03f));
            float3 n = new float3(Hlsl.Floor(q), 31.189f + fi);
            float3 m = Hlsl.Floor(n) * .00001f + Hlsl.Frac(n);
            float3 mp = (31415.9f + m) / Hlsl.Frac(p * m);
            float3 r = Hlsl.Frac(mp);
            float2 s = Hlsl.Abs(Mod(q, 1f) - .5f + .9f * r.XY - .45f); // <<
            s += .01f * Hlsl.Abs(2f * Hlsl.Frac(10f * q.YX) - 1f);
            float d = .6f * Hlsl.Max(s.X - s.Y, s.X + s.Y) + Hlsl.Max(s.X, s.Y) - .01f;
            float edge = .005f + .05f * Hlsl.Min(.5f * Hlsl.Abs(fi - 5f - dof), 1f);
            float tmp = Hlsl.SmoothStep(edge, -edge, d) * (r.X / (1f + .02f * fi * depth));
            acc += new float3(tmp, tmp, tmp);
        }

        // fill scaling
        float screenAspect = (float)DispatchSize.X / DispatchSize.Y;
        float textureAspect = (float)texture.Width / texture.Height;
        float scaleX = 1f, scaleY = 1f;
        if (textureAspect > screenAspect)
            scaleX = screenAspect / textureAspect;
        else
            scaleY = textureAspect / screenAspect;
        UV = new float2(scaleX, scaleY) * (UV - 0.5f) + 0.5f;

        float3 col = texture.Sample(UV + (isBlur ? 0.005f : 0f) * (Rand(UV) - 0.5f)).RGB;

        // Subtle color shift
        col *= Hlsl.Lerp(new float3(1f, 1f, 1f), new float3(.8f, .9f, 1.3f), postProcessing);

        if (this.isLightning)
        {
            // fade in at the start
            float fade = S(0f, 10f, time);
            float light = 0.5f;
            light *= Hlsl.Pow(Hlsl.Max(0f, Hlsl.Sin(time + Hlsl.Sin(time) * 2f)), 10f);
            float val = light * fade;
            col = col * (1f + new float3(val * 0.6f, val * 0.82f, val * 0.9f));
        }

        col = saturation < 1f ? Hlsl.Lerp(Desaturate(col), col, saturation) : col;
        return new float4((acc + col)*brightness, 1f);
    }
}
