using ComputeSharp;
using ComputeSharp.D2D1.Interop;
using ComputeSharp.D2D1;
using System;

namespace Drizzle.UI.UWP.Helpers;

public static class ComputeSharpUtil
{
    public static ReadOnlyTexture2D<Rgba32, float4> CreateTextureOrPlaceholder(string filePath, GraphicsDevice device)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            try
            {
                return device.LoadReadOnlyTexture2D<Rgba32, float4>(filePath);
            }
            catch { /* Nothing to do */ }
        }
        return device.AllocateReadOnlyTexture2D<Rgba32, float4>(1, 1);
    }

    public unsafe static D2D1ResourceTextureManager CreateD2D1ResourceTextureManagerOrPlaceholder(string filePath)
    {
        // As a temporary workaround for the lack of image decoding helpers for D2D1ResourceTextureManager, and in order to
        // keep the code here synchronous and simple, we can just leverage the DirectX 12 APIs to load textures. That is, we
        // first load a texture (which will use WIC behind the scenes) to get the decoded image data in GPU memory, and then
        // copy the data in a readback texture we can read from on the CPU. We can't just load an upload texture and read
        // from it, as that type of texture can only be written to from the CPU. From there, we create a D2D resource texture.
        using ReadOnlyTexture2D<Rgba32, float4> readOnlyTexture = CreateTextureOrPlaceholder(filePath, GraphicsDevice.GetDefault());
        using ReadBackTexture2D<Rgba32> readBackTexture = GraphicsDevice.GetDefault().AllocateReadBackTexture2D<Rgba32>(readOnlyTexture.Width, readOnlyTexture.Height);

        readOnlyTexture.CopyTo(readBackTexture);

        // Get the buffer pointer, the stride, and calculate the buffer size without the trailing padding.
        // That is, the area between the end of the data in the last row and the stride is not included.
        Rgba32* dataBuffer = readBackTexture.View.DangerousGetAddressAndByteStride(out int strideInBytes);
        int bufferSize = ((readBackTexture.Height - 1) * strideInBytes) + (readBackTexture.View.Width * sizeof(Rgba32));

        // Create the resource texture manager to use in the shader
        return new D2D1ResourceTextureManager(
            extents: stackalloc uint[] { (uint)readBackTexture.Width, (uint)readBackTexture.Height },
            bufferPrecision: D2D1BufferPrecision.UInt8Normalized,
            channelDepth: D2D1ChannelDepth.Four,
            filter: D2D1Filter.MinMagMipLinear,
            extendModes: stackalloc D2D1ExtendMode[] { D2D1ExtendMode.Mirror, D2D1ExtendMode.Mirror },
            data: new ReadOnlySpan<byte>(dataBuffer, bufferSize),
            strides: stackalloc uint[] { (uint)strideInBytes });
    }
}
