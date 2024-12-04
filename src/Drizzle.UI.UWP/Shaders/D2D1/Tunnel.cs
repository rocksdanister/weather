using ComputeSharp;
using ComputeSharp.D2D1;

namespace Drizzle.UI.Shaders.D2D1;

/// <summary>
/// A 2D square tunnel.
/// Ported from <see href="https://www.shadertoy.com/view/Ms2SWW"/>.
/// <para>Copyright © 2013 Inigo Quilez.</para>
/// https://www.youtube.com/c/InigoQuilez
/// <para>https://iquilezles.org/</para>
/// <para>The MIT License.</para>
/// </summary>
[D2DInputCount(0)]
[D2DRequiresScenePosition]
[D2DShaderProfile(D2D1ShaderProfile.PixelShader50)]
[AutoConstructor]
public readonly partial struct Tunnel : ID2D1PixelShader
{
    private readonly float time;

    private readonly int2 dispatchSize;

    private readonly float brightness;

    private readonly float speed;

    private readonly bool isSquare;

    [D2DResourceTextureIndex(0)]
    private readonly D2D1ResourceTexture2D<float4> texture;

    const float kPi = 3.1415927f;

    /// <inheritdoc/>
    public float4 Execute()
    {
        float2 fragCoord = new(D2D.GetScenePosition().X, dispatchSize.Y - D2D.GetScenePosition().Y);
        float2 p = (2.0f * fragCoord - dispatchSize.XY) / dispatchSize.Y;

        float a = Hlsl.Atan(p.Y/p.X);

        float r = 0f;
        if (isSquare)
        {
            float2 p2 = p * p, p4 = p2 * p2, p8 = p4 * p4;
            r = Hlsl.Pow(p8.X + p8.Y, 1.0f / 8.0f);
        }
        else
        {
            r = Hlsl.Length(p);
        }

        float2 uv = new float2(0.3f / r + 0.2f * time * speed, a / kPi);

        float3 col = texture.Sample(uv).XYZ;
        col = col * r;

        return new float4(col * brightness, 1f);
    }
}
