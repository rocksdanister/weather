using Microsoft.Graphics.Canvas.UI.Xaml;

namespace Drizzle.UI.UWP.Shaders.D2D1.Runners;

/// <summary>
/// An interface for a shader runner to be used with a Win2D renderer.
/// </summary>
public interface ID2D1ShaderRunner
{
    /// <summary>
    /// Draws a new frame on a target <see cref="ICanvasAnimatedControl"/> instance.
    /// </summary>
    void Execute(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args, double resolutionScale);
}
