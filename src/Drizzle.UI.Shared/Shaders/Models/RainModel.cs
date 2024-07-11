using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Enums;

namespace Drizzle.UI.Shared.Shaders.Models;

public partial class RainModel : BaseModel
{
    [ObservableProperty]
    private float speed  = 0.25f;

    [ObservableProperty]
    private float intensity = 0.4f;

    [ObservableProperty]
    private float zoom = 2.61f;

    [ObservableProperty]
    private float normal = 0.5f;

    [ObservableProperty]
    private float postProcessing = 0.5f;

    [ObservableProperty]
    private bool isPanning = false;

    [ObservableProperty]
    private bool isFreezing = false;

    [ObservableProperty]
    private bool isLightning = false;

    public string ImagePath { get; set; }

    [ObservableProperty]
    private float moveSpeed = 0.5f;

    [ObservableProperty]
    private float inertia = 0.04f;

    public RainModel() : base(ShaderTypes.rain) { }

    public RainModel(RainModel properties) : base(ShaderTypes.rain)
    {
        this.Speed = properties.Speed;
        this.Intensity = properties.Intensity;
        this.Zoom = properties.Zoom;
        this.Normal = properties.Normal;
        this.IsPanning = properties.IsPanning;
        this.IsFreezing = properties.IsFreezing;
        this.IsLightning = properties.IsLightning;
        this.Mouse = properties.Mouse;
        this.ImagePath = properties.ImagePath;
        this.Inertia = properties.Inertia;
        this.MoveSpeed = properties.MoveSpeed;
        this.Saturation = properties.Saturation;
        this.PostProcessing = properties.PostProcessing;
        this.IsDaytime = properties.IsDaytime;
    }
}
