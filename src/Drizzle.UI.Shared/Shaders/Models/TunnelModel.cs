using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Enums;

namespace Drizzle.UI.Shared.Shaders.Models;

public partial class TunnelModel : BaseModel
{
    [ObservableProperty]
    private float speed = 1f;

    [ObservableProperty]
    private bool isSquare = false;

    public string ImagePath { get; set; }

    public TunnelModel() : base(ShaderTypes.tunnel) { }

    public TunnelModel(TunnelModel properties) : base(ShaderTypes.tunnel)
    {
        this.ImagePath = properties.ImagePath;
        this.IsSquare = properties.IsSquare;
        this.speed = properties.Speed;
    }
}
