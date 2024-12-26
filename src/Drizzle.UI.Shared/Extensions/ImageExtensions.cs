using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Drizzle.UI.Shared.Extensions;

public static class ImageExtensions
{
    public static Image<Rgba32> ToImage(this float[] floatArray, int width, int height)
    {
        var image = new Image<Rgba32>(width, height);
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int curPixel = j + i * width;
                image[j, i] = new Rgba32(floatArray[curPixel], floatArray[curPixel], floatArray[curPixel], 1);
            }
        }
        return image;
    }

    /// <summary>
    /// Fit the image within aspect ratio, 
    /// if width > height = (<paramref name="maxDimension"/>, newHeight) else (newWidth, <paramref name="maxDimension"/>)
    /// </summary>
    public static void ResizeMax(this Image image, int maxDimension)
    {
        if (image.Width > maxDimension || image.Height > maxDimension)
        {
            image.Mutate(x =>
            {
                x.Resize(new ResizeOptions()
                {
                    Size = new Size(maxDimension, maxDimension),
                    Mode = ResizeMode.Max
                });
            });
        }
    }
}
