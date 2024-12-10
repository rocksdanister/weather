using ComputeSharp;
using ComputeSharp.D2D1;
using ComputeSharp.D2D1.Interop;
using Drizzle.Models.Enums;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using SixLabors.ImageSharp;
using System;
using System.Threading.Tasks;
using Windows.Graphics.DirectX;
using CSRgba32 = ComputeSharp.Rgba32;
using ISRgba32 = SixLabors.ImageSharp.PixelFormats.Rgba32;

namespace Drizzle.UI.UWP.Helpers;

public static class ComputeSharpUtil
{
    public static ReadOnlyTexture2D<CSRgba32, float4> CreateTextureOrPlaceholder(string filePath, GraphicsDevice device)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            try
            {
                return device.LoadReadOnlyTexture2D<CSRgba32, float4>(filePath);
            }
            catch { /* Nothing to do */ }
        }
        return device.AllocateReadOnlyTexture2D<CSRgba32, float4>(1, 1);
    }

    public static CanvasBitmap CreateCanvasBitmapOrPlaceholder(ICanvasAnimatedControl canvas, string filePath, float dpi = 96)
    {
        using var image = !string.IsNullOrEmpty(filePath) ? Image.Load<ISRgba32>(filePath) : new Image<ISRgba32>(1, 1);

        CanvasBitmap bitmap;
        int width = image.Width;
        int height = image.Height;
        byte[] pixelData = new byte[width * height * 4];

        // Ref: https://learn.microsoft.com/en-us/windows/apps/develop/win2d/pixel-formats
        if (canvas.Device.IsPixelFormatSupported(DirectXPixelFormat.R8G8B8A8UIntNormalized))
        {
            image.CopyPixelDataTo(pixelData);

            bitmap = CanvasBitmap.CreateFromBytes(
                canvas,
                pixelData,
                width,
                height,
                DirectXPixelFormat.R8G8B8A8UIntNormalized, 
                dpi);
        }
        else
        {
            // RGBA -> BGRA
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < width; x++)
                    {
                        var pixel = row[x];
                        int offset = (y * width + x) * 4;

                        pixelData[offset] = pixel.B;     // Blue
                        pixelData[offset + 1] = pixel.G; // Green
                        pixelData[offset + 2] = pixel.R; // Red
                        pixelData[offset + 3] = pixel.A; // Alpha
                    }
                }
            });

            bitmap = CanvasBitmap.CreateFromBytes(
                canvas,
                pixelData,
                width,
                height,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                dpi);
        }
        return bitmap;
    }

    public static async Task<CanvasBitmap> CreateCanvasBitmapOrPlaceholderAsync(CanvasDevice device, string filePath, float dpi = 96)
    {
        if (!string.IsNullOrEmpty(filePath))
            return await CanvasBitmap.LoadAsync(device, filePath, dpi);

        return CanvasBitmap.CreateFromBytes(
            device,
            [0, 0, 0, 255],
            1,
            1,
            DirectXPixelFormat.R8G8B8A8UIntNormalized,
            dpi);
    }

    public static D2D1ResourceTextureManager CreateD2D1ResourceTextureManagerOrPlaceholder(string filePath, TextureWrapMode wrap = TextureWrapMode.clamp)
    {
        using Image<ISRgba32> image = !string.IsNullOrEmpty(filePath) ? Image.Load<ISRgba32>(filePath) : new Image<ISRgba32>(1, 1);
        var width = image.Width;
        var height = image.Height;
        var stride = width * 4;
        var extendMode = GetD2D1ExtendMode(wrap);

        var pixelData = new byte[height * stride];
        image.CopyPixelDataTo(pixelData);

        return new D2D1ResourceTextureManager(
            extents: stackalloc uint[] { (uint)width, (uint)height },
            bufferPrecision: D2D1BufferPrecision.UInt8Normalized,
            channelDepth: D2D1ChannelDepth.Four,
            filter: D2D1Filter.MinMagMipLinear,
            extendModes: stackalloc D2D1ExtendMode[] { extendMode, extendMode },
            data: new ReadOnlySpan<byte>(pixelData),
            strides: stackalloc uint[] { (uint)stride });
    }

    //public unsafe static D2D1ResourceTextureManager CreateD2D1ResourceTextureManagerOrPlaceholder(string filePath, TextureWrapMode wrap = TextureWrapMode.clamp)
    //{
    //    // As a temporary workaround for the lack of image decoding helpers for D2D1ResourceTextureManager, and in order to
    //    // keep the code here synchronous and simple, we can just leverage the DirectX 12 APIs to load textures. That is, we
    //    // first load a texture (which will use WIC behind the scenes) to get the decoded image data in GPU memory, and then
    //    // copy the data in a readback texture we can read from on the CPU. We can't just load an upload texture and read
    //    // from it, as that type of texture can only be written to from the CPU. From there, we create a D2D resource texture.
    //    using ReadOnlyTexture2D<CSRgba32, float4> readOnlyTexture = CreateTextureOrPlaceholder(filePath, GraphicsDevice.GetDefault());
    //    using ReadBackTexture2D<CSRgba32> readBackTexture = GraphicsDevice.GetDefault().AllocateReadBackTexture2D<CSRgba32>(readOnlyTexture.Width, readOnlyTexture.Height);

    //    readOnlyTexture.CopyTo(readBackTexture);

    //    // Get the buffer pointer, the stride, and calculate the buffer size without the trailing padding.
    //    // That is, the area between the end of the data in the last row and the stride is not included.
    //    CSRgba32* dataBuffer = readBackTexture.View.DangerousGetAddressAndByteStride(out int strideInBytes);
    //    int bufferSize = ((readBackTexture.Height - 1) * strideInBytes) + (readBackTexture.View.Width * sizeof(CSRgba32));
    //    var extendMode = GetD2D1ExtendMode(wrap);

    //    // Create the resource texture manager to use in the shader
    //    return new D2D1ResourceTextureManager(
    //        extents: stackalloc uint[] { (uint)readBackTexture.Width, (uint)readBackTexture.Height },
    //        bufferPrecision: D2D1BufferPrecision.UInt8Normalized,
    //        channelDepth: D2D1ChannelDepth.Four,
    //        filter: D2D1Filter.MinMagMipLinear,
    //        extendModes: stackalloc D2D1ExtendMode[] { extendMode, extendMode },
    //        data: new ReadOnlySpan<byte>(dataBuffer, bufferSize),
    //        strides: stackalloc uint[] { (uint)strideInBytes });
    //}

    public static D2D1ExtendMode GetD2D1ExtendMode(TextureWrapMode wrap)
    {
        return wrap switch
        {
            TextureWrapMode.clamp => D2D1ExtendMode.Clamp,
            TextureWrapMode.repeat => D2D1ExtendMode.Wrap,
            TextureWrapMode.mirror => D2D1ExtendMode.Mirror,
            _ => D2D1ExtendMode.Clamp,
        };
    }
}
