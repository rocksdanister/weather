using Avalonia;
using Avalonia.Media;
using Drizzle.Models.Shaders;

namespace Drizzle.UI.Avalonia.UserControls.Shaders;

internal record struct DrawPayload(
    HandlerCommand HandlerCommand,
    ShaderModel? Model = null,
    Size? Size = default,
    Stretch? Stretch = null,
    StretchDirection? StretchDirection = null);
