using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Enums;

namespace Drizzle.UI.Shared.Shaders.Models;

public abstract class BaseModel : ObservableObject
{
    public ShaderTypes Type { get; }

    public float4 Mouse { get; set; } = float4.Zero;

    public float Brightness { get; set; } = 1f;

    public float Saturation { get; set; } = 1f;

    public float TimeMultiplier { get; set; } = 1f;

    public bool IsDaytime { get; set; } = true;

    protected BaseModel(ShaderTypes type)
    {
        this.Type = type;
    }
}
