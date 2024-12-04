namespace Drizzle.Models.Enums;

public enum ShaderQuality
{
    /// <summary>
    /// No quality operation performed.
    /// </summary>
    none,
    /// <summary>
    /// Use optimised defaults from ShaderModel.
    /// </summary>
    optimized,
    /// <summary>
    /// Maximum quality.
    /// </summary>
    maximum,
    /// <summary>
    /// Adjust quality to meet TargetFramerate.
    /// </summary>
    dynamic
}