using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Enums;

namespace Drizzle.UI.Shared.Shaders.Models;

public partial class WindModel : BaseModel
{
    [ObservableProperty]
    private float3 color1 = new(0.75f, 0.75f, 0.75f);

    [ObservableProperty]
    private float3 color2 = new(0f, 0f, 0f);

    [ObservableProperty]
    private float speed = 5f;

    [ObservableProperty]
    private float amplitude = 0.5f;

    // Depth parallax
    [ObservableProperty]
    private float parallaxIntensityX = 0.5f;

    [ObservableProperty]
    private float parallaxIntensityY = 0.5f;

    public float MaxSpeed { get; } = 10f;

    public float MinSpeed { get; } = 0.1f;

    public string ImagePath { get; set; }

    public string DepthPath { get; set; }

    public WindModel() : base(ShaderTypes.fog) { }

    public WindModel(WindModel properties) : base(ShaderTypes.fog)
    {
        this.Color1 = properties.Color1;
        this.color2 = properties.Color2;
        this.speed = properties.Speed;
        this.Amplitude = properties.Amplitude;
        this.ImagePath = properties.ImagePath;
        this.DepthPath = properties.DepthPath;
        this.Saturation = properties.Saturation;
        this.parallaxIntensityX = properties.ParallaxIntensityX;
        this.parallaxIntensityY = properties.ParallaxIntensityY;
        this.IsDaytime = properties.IsDaytime;
    }

}
