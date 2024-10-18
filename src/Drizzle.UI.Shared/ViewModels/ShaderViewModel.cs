using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Shaders;
using Drizzle.Models.Enums;
using System;

#if WINDOWS_UWP
using ComputeSharp.Uwp;
#endif

namespace Drizzle.UI.Shared.ViewModels;

public partial class ShaderViewModel : ObservableObject
{
    public ShaderModel Model { get; set; }

#if WINDOWS_UWP
    public IShaderRunner Runner { get; set; }
#endif
}
