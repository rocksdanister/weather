using ComputeSharp;
using ComputeSharp.D2D1;

namespace Drizzle.UI.Shaders.D2D1;

/// <summary>
/// Fully procedural 3D animated volume with three evaluations per step (for shading).
/// Ported from <see href="https://www.shadertoy.com/view/3l23Rh"/>.
/// <para>Created by nimitz (twitter: @stormoid).</para>
/// <para>License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.</para>
/// </summary>
[D2DInputCount(0)]
[D2DRequiresScenePosition]
[D2DShaderProfile(D2D1ShaderProfile.PixelShader50)]
[AutoConstructor]
public readonly partial struct Clouds : ID2D1PixelShader
{
    /// <summary>
    /// The current time Hlsl.Since the start of the application.
    /// </summary>
    private readonly float time;

    /// <summary>
    /// The dispatch size for the current output.
    /// </summary>
    private readonly int2 dispatchSize;

    private readonly float4 mouse;

    private readonly float speed;

    private readonly float scale;

    private readonly int iterations;

    private readonly float brightness;

    private readonly float saturation;

    private readonly bool isDaytime;

    private readonly bool isDayNightShift;

    private float Time() => 1000f + time * speed;

    private static readonly float3x3 M3 = new float3x3(0.33338f, 0.56034f, -0.71817f, -0.87887f, 0.32651f, -0.15323f, 0.15162f, 0.69596f, 0.61339f) * 1.93f;

    const float zoom = 1.05f;

    private static float2x2 Rotate(in float a)
    {
        float c = Hlsl.Cos(a);
        float s = Hlsl.Sin(a);

        return new(c, s, -s, c);
    }

    private static float LinStep(in float mn, in float mx, in float x)
    {
        return Hlsl.Clamp((x - mn) / (mx - mn), 0.0f, 1.0f);
    }

    private static float2 Disp(float t)
    {
        return new float2(Hlsl.Sin(t * 0.22f) * 1.0f, Hlsl.Cos(t * 0.175f) * 1.0f) * 2.0f;
    }

    private float2 Map(float3 p, float prm1, float2 bsMo)
    {
        float3 p2 = p;

        p2.XY -= Disp(p.Z).XY;
        p.XY = Hlsl.Mul(p.XY, Rotate((Hlsl.Sin(p.Z + Time()) * (0.1f + (prm1 * 0.05f))) + (Time() * 0.09f)));

        float cl = Hlsl.Dot(p2.XY, p2.XY);
        float d = 0.0f;

        p *= this.scale;

        float z = 1.0f;
        float trk = 1.0f;
        float dspAmp = 0.1f + (prm1 * 0.2f);

        for (int i = 0; i < this.iterations; i++)
        {
            p += Hlsl.Sin((p.ZXY * 0.75f * trk) + (Time() * trk * 0.8f)) * dspAmp;
            d -= Hlsl.Abs(Hlsl.Dot(Hlsl.Cos(p), Hlsl.Sin(p.YZX)) * z);
            z *= 0.57f;
            trk *= 1.4f;

            p = Hlsl.Mul(p, M3);
        }

        d = Hlsl.Abs(d + (prm1 * 3.0f)) + (prm1 * 0.3f) - 2.5f + bsMo.Y;

        return new float2(d + (cl * 0.2f) + 0.25f, cl);
    }

    private float4 Render(float3 ro, float3 rd, float time, float prm1, float2 bsMo)
    {
        float4 rez = 0;
        float t = 1.5f;
        float fogT = 0;

        for (int i = 0; i < 130; i++)
        {
            if (rez.A > 0.99f)
            {
                break;
            }

            float3 pos = ro + (t * rd);
            float2 mpv = Map(pos, prm1, bsMo);
            float den = Hlsl.Clamp(mpv.X - 0.3f, 0.0f, 1.0f) * 1.12f;
            float dn = Hlsl.Clamp(mpv.X + 2.0f, 0.0f, 3.0f);
            float4 col = 0f;

            if (mpv.X > 0.6f)
            {

                col = new float4((Hlsl.Sin(new float3(5.0f, 0.4f, 0.2f) + (mpv.Y * 0.1f) + (Hlsl.Sin(pos.Z * 0.4f) * 0.5f) + 1.8f) * 0.5f) + 0.5f, 0.08f);
                col *= den * den * den;
                col.RGB *= LinStep(4.0f, -2.5f, mpv.X) * 2.3f;

                float dif = Hlsl.Clamp((den - Map(pos + 0.8f, prm1, bsMo).X) / 9.0f, 0.001f, 1.0f);

                dif += Hlsl.Clamp((den - Map(pos + 0.35f, prm1, bsMo).X) / 2.5f, 0.001f, 1.0f);
                col.XYZ *= den * (new float3(0.005f, 0.045f, 0.075f) + (1.5f * new float3(0.033f, 0.07f, 0.03f) * dif));
            }

            float fogC = Hlsl.Exp((t * 0.2f) - 2.2f);

            col += new float4(0.06f, 0.11f, 0.11f, 0.1f) * Hlsl.Clamp(fogC - fogT, 0.0f, 1.0f);
            fogT = fogC;
            rez += col * (1.0f - rez.A);
            t += Hlsl.Clamp(0.5f - (dn * dn * 0.05f), 0.09f, 0.3f);
        }

        return Hlsl.Clamp(rez, 0.0f, 1.0f);
    }

    private static float GetSaturation(float3 c)
    {
        float mi = Hlsl.Min(Hlsl.Min(c.X, c.Y), c.Z);
        float ma = Hlsl.Max(Hlsl.Max(c.X, c.Y), c.Z);

        return (ma - mi) / (ma + (float)1e-7);
    }

    private static float3 ILerp(in float3 a, in float3 b, in float x)
    {
        float3 ic = Hlsl.Lerp(a, b, x) + new float3((float)1e-6, 0.0f, 0.0f);
        float sd = Hlsl.Abs(GetSaturation(ic) - Hlsl.Lerp(GetSaturation(a), GetSaturation(b), x));
        float3 dir = Hlsl.Normalize(new float3((2.0f * ic.X) - ic.Y - ic.Z, (2.0f * ic.Y) - ic.X - ic.Z, (2.0f * ic.Z) - ic.Y - ic.X));
        float lgt = Hlsl.Dot(1.0f, ic);
        float ff = Hlsl.Dot(dir, Hlsl.Normalize(ic));

        ic += 1.5f * dir * sd * ff * lgt;

        return Hlsl.Clamp(ic, 0.0f, 1.0f);
    }

    //ref: https://stackoverflow.com/questions/9320953/what-algorithm-does-photoshop-use-to-desaturate-an-image
    float3 Desaturate(float3 color)
    {
        float bw = (Hlsl.Min(color.R, Hlsl.Min(color.G, color.B)) + Hlsl.Min(color.R, Hlsl.Min(color.G, color.B))) * 0.5f;
        return new float3(bw, bw, bw);
    }

    /// <inheritdoc/>
    public float4 Execute()
    {
        int2 xy = (int2)D2D.GetScenePosition().XY;
        float2 q = (float2)xy / this.dispatchSize;
        // Scale to hide edges.
        q = (q - 0.5f) / zoom + 0.5f;
        float2 p = (xy - (0.5f * (float2)this.dispatchSize)) / this.dispatchSize.Y;
        float2 bsMo = mouse.XY - 0.5f * (float2)this.dispatchSize / this.dispatchSize.Y;
        float scaledTime = Time() * 3.0f;
        float3 ro = new(0, 0, scaledTime);

        ro += new float3(Hlsl.Sin(Time()) * 0.5f, Hlsl.Sin(Time() * 1.0f) * 0.0f, 0);

        float dspAmp = 0.85f;

        ro.XY += Disp(ro.Z) * dspAmp;

        float tgtDst = 3.5f;
        float3 target = Hlsl.Normalize(ro - new float3(Disp(scaledTime + tgtDst) * dspAmp, scaledTime + tgtDst));

        ro.X -= bsMo.X * 2.0f;

        float3 rightdir = Hlsl.Normalize(Hlsl.Cross(target, new float3(0, 1, 0)));
        float3 updir = Hlsl.Normalize(Hlsl.Cross(rightdir, target));

        rightdir = Hlsl.Normalize(Hlsl.Cross(updir, target));

        float3 rd = Hlsl.Normalize((((p.X * rightdir) + (p.Y * updir)) * 1.0f) - target);

        rd.XY = Hlsl.Mul(rd.XY, Rotate((-Disp(scaledTime + 3.5f).X * 0.2f) + bsMo.X));

        float timeStep = isDaytime ? 0.75f : 0.25f;
        float prm1 = isDayNightShift ? timeStep : Hlsl.SmoothStep(-0.4f, 0.4f, Hlsl.Sin(Time() * 0.3f));
        float4 scn = Render(ro, rd, scaledTime, prm1, bsMo);
        float3 col = scn.RGB;

        col = ILerp(col.BGR, col.RGB, Hlsl.Clamp(1.0f - prm1, 0.05f, 1.0f));
        col = Hlsl.Pow(col, new float3(0.55f, 0.65f, 0.6f)) * new float3(1.0f, 0.97f, 0.9f);
        col *= (Hlsl.Pow(Hlsl.Abs(16.0f * q.X * q.Y * (1.0f - q.X) * (1.0f - q.Y)), 0.12f) * 0.7f) + 0.3f;

        col = saturation < 1f ? Hlsl.Lerp(Desaturate(col), col, saturation) : col;
        return new(col * brightness, 1.0f);
    }
}