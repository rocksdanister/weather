using ComputeSharp;

namespace Drizzle.UI.Shared.Shaders;

/// <summary>
/// Rain effect shader
/// Ported from <see href="https://www.shadertoy.com/view/ltffzl"/>.
/// <para>Created by Martijn Steinrucken aka BigWings - 2017(Twitter:@The_ArtOfCode).</para>
/// <para>License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.</para>
/// </summary>
[AutoConstructor]
[EmbeddedBytecode(DispatchAxis.XY)]
public readonly partial struct Rain : IPixelShader<float4>
{
    /// <summary>
    /// The current time Hlsl.Since the start of the application.
    /// </summary>
    private readonly float time;

    private readonly float4 mouse;

    private readonly float speed;

    private readonly float intensity;

    private readonly float zoom;

    private readonly float normal;

    private readonly float brightness;

    private readonly float saturation;

    private readonly float postProcessing;

    private readonly bool isPanning;

    private readonly bool isFreezing;

    private readonly bool isLightning;

    private readonly IReadOnlyNormalizedTexture2D<float4> texture;

    //#define S(a, b, t) smoothstep(a, b, t)
    float S(float a, float b, float t)
    {
        return Hlsl.SmoothStep(a, b, t);
    }

    // from DAVE HOSKINS
    float3 N13(float p)
    {
        float3 p3 = Hlsl.Frac(new float3(p, p, p) * new float3(.1031f, .11369f, .13787f));
        p3 += Hlsl.Dot(p3, p3.YZX + 19.19f);
        return Hlsl.Frac(new float3((p3.X + p3.Y) * p3.Z, (p3.X + p3.Z) * p3.Y, (p3.Y + p3.Z) * p3.X));
    }

    float4 N14(float t)
    {
        return Hlsl.Frac(Hlsl.Sin(t * new float4(123f, 1024f, 1456f, 264f)) * new float4(6547f, 345f, 8799f, 1564f));
    }

    float N(float t)
    {
        return Hlsl.Frac(Hlsl.Sin(t * 12345.564f) * 7658.76f);
    }

    float Saw(float b, float t)
    {
        return S(0f, b, t) * S(1f, b, t);
    }

    float2 DropLayer2(float2 uv, float t)
    {
        float2 UV = uv;

        uv.Y += t * 0.75f;
        float2 a = new float2(6f, 1f);
        float2 grid = a * 2f;
        float2 id = Hlsl.Floor(uv * grid);

        float colShift = N(id.X);
        uv.Y += colShift;

        id = Hlsl.Floor(uv * grid);
        float3 n = N13(id.X * 35.2f + id.Y * 2376.1f);
        float2 st = Hlsl.Frac(uv * grid) - new float2(.5f, 0f);

        float x = n.X - .5f;

        float y = UV.Y * 20f;
        float wiggle = Hlsl.Sin(y + Hlsl.Sin(y));
        x += wiggle * (.5f - Hlsl.Abs(x)) * (n.Z - .5f);
        x *= .7f;
        float ti = Hlsl.Frac(t + n.Z);
        y = (Saw(.85f, ti) - .5f) * .9f + .5f;
        float2 p = new float2(x, y);

        float d = Hlsl.Length((st - p) * a.YX);

        float mainDrop = S(.4f, .0f, d);

        float r = Hlsl.Sqrt(S(1f, y, st.Y));
        float cd = Hlsl.Abs(st.X - x);
        float trail = S(.23f * r, .15f * r * r, cd);
        float trailFront = S(-.02f, .02f, st.Y - y);
        trail *= trailFront * r * r;

        y = UV.Y;
        float trail2 = S(.2f * r, .0f, cd);
        float droplets = Hlsl.Max(0f, (Hlsl.Sin(y * (1f - y) * 120f) - st.Y)) * trail2 * trailFront * n.Z;
        y = Hlsl.Frac(y * 10f) + (st.Y - .5f);
        float dd = Hlsl.Length(st - new float2(x, y));
        droplets = S(.3f, 0f, dd);
        float m = mainDrop + droplets * r * trailFront;

        //m += st.x>a.y*.45 || st.y>a.x*.165 ? 1.2 : 0.;
        return new float2(m, trail);
    }

    float StaticDrops(float2 uv, float t)
    {
        uv *= 40f;

        float2 id = Hlsl.Floor(uv);
        uv = Hlsl.Frac(uv) - .5f;
        float3 n = N13(id.X * 107.45f + id.Y * 3543f);
        float2 p = (n.XY - .5f) * .7f;
        float d = Hlsl.Length(uv - p);

        float fade = Saw(.025f, Hlsl.Frac(t + n.Z));
        float c = S(.3f, 0f, d) * Hlsl.Frac(n.Z * 10f) * fade;
        return c;
    }

    float2 Drops(float2 uv, float t, float l0, float l1, float l2)
    {
        float s = StaticDrops(uv, t) * l0;
        float2 m1 = DropLayer2(uv, t) * l1;
        float2 m2 = DropLayer2(uv * 1.85f, t) * l2;

        float c = s + m1.X + m2.X;
        c = S(.3f, 1f, c);

        return new float2(c, Hlsl.Max(m1.Y * l0, m2.Y * l1));
    }

    //random no.
    float N21(float2 p)
    {
        p = Hlsl.Frac(p * new float2(123.34f, 345.45f));
        p += Hlsl.Dot(p, p + 34.345f);
        return Hlsl.Frac(p.X * p.Y);
    }

    //ref: https://stackoverflow.com/questions/9320953/what-algorithm-does-photoshop-use-to-desaturate-an-image
    float3 Desaturate(float3 color)
    {
        float bw = (Hlsl.Min(color.R, Hlsl.Min(color.G, color.B)) + Hlsl.Min(color.R, Hlsl.Min(color.G, color.B))) * 0.5f;
        return new float3(bw, bw, bw);
    }

    float Rand(float2 co)
    {
        return Hlsl.Frac(Hlsl.Sin(Hlsl.Dot(co, new float2(12.9898f, 78.233f))) * 43758.5453f);
    }

    public float4 Execute()
    {
        float2 fragCoord = new(ThreadIds.X, DispatchSize.Y - ThreadIds.Y);
        float2 uv = new float2(mouse.X, -1*mouse.Y) + (fragCoord.XY - (float2)DispatchSize.XY) / DispatchSize.Y;
        float2 UV = fragCoord.XY / DispatchSize.XY;//-.5;
        UV.Y = 1f - UV.Y;
        float T = time + (mouse.Y + mouse.X) * 4f;

        // fill scaling
        float screenAspect = (float)DispatchSize.X / DispatchSize.Y;
        float textureAspect = (float)texture.Width / texture.Height;
        float scaleX = 1f, scaleY = 1f;
        if (textureAspect > screenAspect)
            scaleX = screenAspect / textureAspect;
        else
            scaleY = textureAspect / screenAspect;
        UV = new float2(scaleX, scaleY) * (UV - 0.5f) + 0.5f;

        float t = T * .2f * this.speed;
        float rainAmount = this.intensity;

        float view = this.isPanning ? -Hlsl.Cos(T * .2f) : 0f;
        uv *= (.7f + view * .3f) * this.zoom;

        float staticDrops = S(-.5f, 1f, rainAmount) * 2f;
        float layer1 = S(.25f, .75f, rainAmount);
        float layer2 = S(.0f, .5f, rainAmount);

        float2 c = Drops(uv, t, staticDrops, layer1, layer2);
        float2 e = new float2(.001f, 0f) * this.normal;
        float cx = Drops(uv + e, t, staticDrops, layer1, layer2).X;
        float cy = Drops(uv + e.YX, t, staticDrops, layer1, layer2).X;
        float2 n = new float2(cx - c.X, cy - c.X);      // expensive normals

        float3 col = texture.Sample(UV + n + (isFreezing ? 0.01f : 0f) * (Rand(UV) - 0.5f)).RGB;//texture.Sample(UV + n).RGB;

        // make time sync with first lightnoing
        t = (T + 3f) * 1f;
        // subtle color shift
        col *= Hlsl.Lerp(new float3(1f, 1f, 1f), new float3(.8f, .9f, 1.3f), postProcessing);

        if (this.isLightning)
        {
            // fade in at the start
            float fade = S(0f, 10f, T);

            //float light = Hlsl.Sin(t * Hlsl.Sin(t * 10f));                // lighting flicker
            //light *= Hlsl.Pow(Hlsl.Max(0f, Hlsl.Sin(t + Hlsl.Sin(t))), 10f);        // lightning flash
            //col *= 1f + light * fade * Hlsl.Lerp(1f, .1f, 0f); // composite lightning

            float light = 0.5f;
            light *= Hlsl.Pow(Hlsl.Max(0f, Hlsl.Sin(t + Hlsl.Sin(t)*2f)), 10f);
            float val = light * fade;
            col = col * (1f + new float3(val * 0.6f, val * 0.82f, val * 0.9f));
        }
        UV -= .5f;
        col *= 1f - Hlsl.Dot(UV, UV) * 1f; // vignette

        col = saturation < 1f ? Hlsl.Lerp(Desaturate(col), col, saturation) : col;
        return new float4(col*brightness *0.9f, 1f);
    }
}
