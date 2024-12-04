using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Enums;
using Drizzle.Models.Shaders.Uniform;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Drizzle.Models.Shaders;

public partial class WindModel : ShaderModel
{
    [ObservableProperty]
    private Vector3 color1 = new(0.75f, 0.75f, 0.75f);

    [ObservableProperty]
    private Vector3 color2 = Vector3.Zero;

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

    public string? ImagePath { get; set; }

    public string? DepthPath { get; set; }

    public WindModel(Uri shaderUri, WindModel properties) : base(
        shaderUri ?? properties?.ShaderUri,
        ShaderTypes.fog,
        scaleFactor: 0.75f,
        maxScaleFactor: 1f,
        mouseSpeed: -0.075f,
        mouseInertia: 0.08f)
    {
        if (properties != null)
        {
            this.Color1 = properties.Color1;
            this.Color2 = properties.Color2;
            this.Speed = properties.Speed;
            this.Amplitude = properties.Amplitude;
            this.ImagePath = properties.ImagePath;
            this.DepthPath = properties.DepthPath;
            this.Saturation = properties.Saturation;
            this.ParallaxIntensityX = properties.ParallaxIntensityX;
            this.ParallaxIntensityY = properties.ParallaxIntensityY;
            this.IsDaytime = properties.IsDaytime;
        }

        InitializeUniformMappings();
    }

    public WindModel(Uri shaderUri) : this(shaderUri, null) { }

    public WindModel() : this(null, null) { }

    public WindModel(WindModel properties) : this(null, properties) { }

    protected override void InitializeUniformMappings()
    {
        base.InitializeUniformMappings();

        AddUniformMappings(new Dictionary<string, UniformProperty>
        {
            {
                nameof(Color1), new ColorProperty {
                    UniformName = "u_Color1",
                    GetValue = model => ((WindModel)model).Color1,
                    SetValue = (model, value) => ((WindModel)model).Color1 = (Vector3)value
                }
            },
            {
               nameof(Color2), new ColorProperty {
                    UniformName = "u_Color2",
                    GetValue = model => ((WindModel)model).Color2,
                    SetValue = (model, value) => ((WindModel)model).Color2 = (Vector3)value
                }
            },
            {
                nameof(Speed), new FloatProperty {
                    UniformName = "u_Speed",
                    GetValue = model => ((WindModel)model).Speed,
                    SetValue = (model, value) => ((WindModel)model).Speed = (float)value
                }
            },
            {
               nameof(Amplitude), new FloatProperty {
                    UniformName = "u_Amplitude",
                    GetValue = model => ((WindModel)model).Amplitude,
                    SetValue = (model, value) => ((WindModel)model).Amplitude = (float)value
                }
            },
            {
               nameof(ParallaxIntensityX), new FloatProperty {
                    UniformName = "u_ParallaxIntensityX",
                    GetValue = model => ((WindModel)model).ParallaxIntensityX,
                    SetValue = (model, value) => ((WindModel)model).ParallaxIntensityX = (float)value
                }
            },
            {
               nameof(ParallaxIntensityY), new FloatProperty {
                    UniformName = "u_ParallaxIntensityY",
                    GetValue = model => ((WindModel)model).ParallaxIntensityY,
                    SetValue = (model, value) => ((WindModel)model).ParallaxIntensityY = (float)value
                }
            },
            {
                nameof(ImagePath), new TextureProperty {
                    UniformName = "u_Texture",
                    GetValue = model => ((WindModel)model).ImagePath,
                    SetValue = (model, value) => ((WindModel)model).ImagePath = (string)value
                }
            },
            {
                nameof(DepthPath), new TextureProperty {
                    UniformName = "u_DepthTexture",
                    GetValue = model => ((WindModel)model).DepthPath,
                    SetValue = (model, value) => ((WindModel)model).DepthPath = (string)value
                }
            }
        });
    }
}
