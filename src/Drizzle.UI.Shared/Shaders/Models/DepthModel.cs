using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Enums;

namespace Drizzle.UI.Shared.Shaders.Models;

public partial class DepthModel : BaseModel
{
    // Binding not working for Float2
    [ObservableProperty]
    private float intensityX = 0.5f;

    [ObservableProperty]
    private float intensityY = 0.5f;

    [ObservableProperty]
    private bool isBlur = false;

    public string ImagePath { get; set; }

    public string DepthPath { get; set; }

    public DepthModel() : base(ShaderTypes.depth) { }

    public DepthModel(DepthModel properties) : base(ShaderTypes.depth)
    {
        this.Mouse = properties.Mouse;
        this.IntensityX = properties.IntensityX;
        this.intensityY = properties.IntensityY;
        this.ImagePath = properties.ImagePath;
        this.DepthPath = properties.DepthPath;
        this.Saturation = properties.Saturation;
        this.IsDaytime = properties.IsDaytime;
    }
}
