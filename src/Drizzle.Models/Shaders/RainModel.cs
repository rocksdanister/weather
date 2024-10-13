using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Enums;
using Drizzle.Models.Shaders.Uniform;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Drizzle.Models.Shaders;

public partial class RainModel : ShaderModel
{
    [ObservableProperty]
    private float speed = 0.25f;

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

    /// <summary>
    /// Use N14 random function for static drops (Apply M1 Mac black spot patch.)
    /// </summary>
    public bool IsRandomN14 { get; private set; } = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public string? ImagePath { get; set; } = null;

    public RainModel(Uri shaderUri) : base(shaderUri, ShaderTypes.rain, scaleFactor: 0.75f, maxScaleFactor: 1f, mouseSpeed: 0.5f, mouseInertia: 0.04f)
    {
        InitializeUniformMappings();
    }

    public RainModel() : base(null, ShaderTypes.rain, scaleFactor: 0.75f, maxScaleFactor: 1f, mouseSpeed: 0.5f, mouseInertia: 0.04f) 
    {
        InitializeUniformMappings();
    }

    public RainModel(RainModel properties) : base(properties.ShaderUri, ShaderTypes.rain, scaleFactor: 0.75f, maxScaleFactor: 1f, mouseSpeed: 0.5f, mouseInertia: 0.04f)
    {
        this.Speed = properties.Speed;
        this.MouseSpeed = properties.MouseSpeed;
        this.MouseInertia = properties.MouseInertia;
        this.Intensity = properties.Intensity;
        this.Zoom = properties.Zoom;
        this.Normal = properties.Normal;
        this.IsPanning = properties.IsPanning;
        this.IsFreezing = properties.IsFreezing;
        this.IsLightning = properties.IsLightning;
        this.Mouse = properties.Mouse;
        this.ImagePath = properties.ImagePath;
        this.Saturation = properties.Saturation;
        this.PostProcessing = properties.PostProcessing;
        this.IsDaytime = properties.IsDaytime;

        InitializeUniformMappings();
    }

    protected override void InitializeUniformMappings()
    {
        base.InitializeUniformMappings();

        AddUniformMappings(new Dictionary<string, UniformProperty>
        {
            {
                nameof(Speed), new FloatProperty {
                    UniformName = "u_Speed",
                    GetValue = model => ((RainModel)model).Speed,
                    SetValue = (model, value) => ((RainModel)model).Speed = (float)value
                }
            },
            {
                nameof(Intensity), new FloatProperty {
                    UniformName = "u_Intensity",
                    LerpSpeed = 0.05f,
                    GetValue = model => ((RainModel)model).Intensity,
                    SetValue = (model, value) => ((RainModel)model).Intensity = (float)value
                }
            },
            {
                nameof(Zoom), new FloatProperty {
                    UniformName = "u_Zoom",
                    LerpSpeed = 0.1f,
                    GetValue = model => ((RainModel)model).Zoom,
                    SetValue = (model, value) => ((RainModel)model).Zoom = (float)value
                }
            },
            {
                nameof(Normal), new FloatProperty {
                    UniformName = "u_Normal",
                    LerpSpeed = 0.05f,
                    GetValue = model => ((RainModel)model).Normal,
                    SetValue = (model, value) => ((RainModel)model).Normal = (float)value
                }
            },
            {
                nameof(PostProcessing), new FloatProperty {
                    UniformName = "u_PostProcessing",
                    LerpSpeed = 0.05f,
                    GetValue = model => ((RainModel)model).PostProcessing,
                    SetValue = (model, value) => ((RainModel)model).PostProcessing = (float)value
                }
            },
            {
                nameof(IsPanning), new BoolProperty {
                    UniformName = "u_IsPanning",
                    GetValue = model => ((RainModel)model).IsPanning,
                    SetValue = (model, value) => ((RainModel)model).IsPanning = (bool)value
                }
            },
            {
                nameof(IsFreezing), new BoolProperty {
                    UniformName = "u_IsFreezing",
                    GetValue = model => ((RainModel)model).IsFreezing,
                    SetValue = (model, value) => ((RainModel)model).IsFreezing = (bool)value
                }
            },
            {
                nameof(IsLightning), new BoolProperty {
                    UniformName = "u_IsLightning",
                    GetValue = model => ((RainModel)model).IsLightning,
                    SetValue = (model, value) => ((RainModel)model).IsLightning = (bool)value
                }
            },
            {
                nameof(IsRandomN14), new BoolProperty {
                    UniformName = "u_IsRandomN14",
                    GetValue = model => ((RainModel)model).IsRandomN14,
                    SetValue = (model, value) => ((RainModel)model).IsRandomN14 = (bool)value
                }
            },
            {
                nameof(ImagePath), new TextureProperty {
                    UniformName = "u_Texture",
                    GetValue = model => ((RainModel)model).ImagePath,
                    SetValue = (model, value) => ((RainModel)model).ImagePath = (string)value
                }
            }
        });
    }
}
