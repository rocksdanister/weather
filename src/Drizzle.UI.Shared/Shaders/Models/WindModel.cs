using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Common;
using Drizzle.UI.Shared.Shaders.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
    private float parallaxIntensityX = 0.75f;

    [ObservableProperty]
    private float parallaxIntensityY = 1f;

    public float MaxSpeed { get; } = 10f;

    public float MinSpeed { get; } = 0.1f;

    public string ImagePath { get; set; }

    public string DepthPath { get; set; }

    public static float DefaultBrightness { get; } = 0.75f;

    public static float DefaultSaturation { get; } = 1f;

    public WindModel() : base(ShaderTypes.fog, DefaultBrightness, DefaultSaturation) { }

    public WindModel(WindModel obj) : base(ShaderTypes.fog, DefaultBrightness, DefaultSaturation)
    {
        this.Color1 = obj.Color1;
        this.color2 = obj.Color2;
        this.speed = obj.Speed;
        this.Amplitude = obj.Amplitude;
        this.ImagePath = obj.ImagePath;
        this.DepthPath = obj.DepthPath;
        this.Saturation = obj.Saturation;
        this.parallaxIntensityX = obj.ParallaxIntensityX;
        this.parallaxIntensityY = obj.ParallaxIntensityY;
    }

}
