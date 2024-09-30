using ComputeSharp;

namespace Drizzle.UI.UWP.Helpers
{
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
    }
}
