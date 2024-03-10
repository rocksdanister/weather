using ComputeSharp;

namespace Drizzle.UI.Shared.Shaders;

/// <summary>
/// A 2D square tunnel.
/// Ported from <see href="https://www.shadertoy.com/view/Ms2SWW"/>.
/// <para>Copyright © 2013 Inigo Quilez.</para>
/// https://www.youtube.com/c/InigoQuilez
/// <para>https://iquilezles.org/</para>
/// <para>The MIT License.</para>
/// </summary>
[AutoConstructor]
[EmbeddedBytecode(DispatchAxis.XY)]
public readonly partial struct Tunnel : IPixelShader<float4>
{
    /// <summary>
    /// The current time Hlsl.Since the start of the application.
    /// </summary>
    private readonly float time;

    private readonly IReadOnlyNormalizedTexture2D<float4> texture;

    private readonly float brightness;

    private readonly float speed;

    private readonly bool isSquare;

    const float kPi = 3.1415927f;

    /// <inheritdoc/>
    public float4 Execute()
    {
        float2 fragCoord = new(ThreadIds.X, DispatchSize.Y - ThreadIds.Y);
        float2 p = (2.0f * fragCoord - DispatchSize.XY) / DispatchSize.Y;

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
