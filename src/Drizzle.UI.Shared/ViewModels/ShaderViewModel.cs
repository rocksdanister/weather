using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Shaders;

#if WINDOWS_UWP
using ComputeSharp.Uwp;
using Drizzle.UI.UWP.Shaders.D2D1.Common;
#endif

namespace Drizzle.UI.Shared.ViewModels;

public partial class ShaderViewModel : ObservableObject
{
    public ShaderModel Model { get; set; }

#if WINDOWS_UWP
    public IShaderRunner Runner { get; set; }
    public ID2D1ShaderRunner D2D1ShaderRunner { get; set; }
#endif
}
