using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Enums;
using Drizzle.Models.Shaders.Uniform;
using System;
using System.Collections.Generic;

namespace Drizzle.Models.Shaders;

public partial class SnowModel : ShaderModel
{
    [ObservableProperty]
    private float speed = 0.6f;

    [ObservableProperty]
    private float depth = 1f;

    [ObservableProperty]
    private float width = 0.3f;

    [ObservableProperty]
    private int layers = 25;

    [ObservableProperty]
    private float postProcessing = 0.5f;

    [ObservableProperty]
    private bool isLightning = false;

    [ObservableProperty]
    private bool isBlur = true;

    public string? ImagePath { get; set; } = null;

    public SnowModel(Uri shaderUri, SnowModel properties) : base(
        shaderUri ?? properties?.ShaderUri,
        ShaderTypes.snow,
        scaleFactor: 0.75f,
        maxScaleFactor: 1f,
        mouseSpeed: 1.5f,
        mouseInertia: 0.08f)
    {
        if (properties != null)
        {
            this.Speed = properties.Speed;
            this.MouseSpeed = properties.MouseSpeed;
            this.MouseInertia = properties.MouseInertia;
            this.TimeMultiplier = properties.TimeMultiplier;
            this.Depth = properties.Depth;
            this.Width = properties.Width;
            this.Layers = properties.Layers;
            this.Mouse = properties.Mouse;
            this.IsBlur = properties.IsBlur;
            this.Saturation = properties.Saturation;
            this.IsLightning = properties.IsLightning;
            this.PostProcessing = properties.PostProcessing;
            this.IsDaytime = properties.IsDaytime;
        }

        InitializeUniformMappings();
    }

    public SnowModel(Uri shaderUri) : this(shaderUri, null) { }

    public SnowModel() : this(null, null) { }

    public SnowModel(SnowModel properties) : this(null, properties) { }

    protected override void InitializeUniformMappings()
    {
        base.InitializeUniformMappings();

        AddUniformMappings(new Dictionary<string, UniformProperty>
        {
            {
                nameof(Speed), new FloatProperty {
                    UniformName = "u_Speed",
                    GetValue = model => ((SnowModel)model).Speed,
                    SetValue = (model, value) => ((SnowModel)model).Speed = (float)value
                }
            },
            {
                nameof(Depth), new FloatProperty {
                    UniformName = "u_Depth",
                    GetValue = model => ((SnowModel)model).Depth,
                    SetValue = (model, value) => ((SnowModel)model).Depth = (float)value
                }
            },
            {
                nameof(Width), new FloatProperty {
                    UniformName = "u_Width",
                    LerpSpeed = 0.01f,
                    GetValue = model => ((SnowModel)model).Width,
                    SetValue = (model, value) => ((SnowModel)model).Width = (float)value
                }
            },
            {
                nameof(Layers), new IntProperty {
                    UniformName = "u_Layers",
                    GetValue = model => ((SnowModel)model).Layers,
                    SetValue = (model, value) => ((SnowModel)model).Layers = (int)value
                }
            },
            {
                nameof(PostProcessing), new FloatProperty {
                    UniformName = "u_PostProcessing",
                    LerpSpeed = 0.05f,
                    GetValue = model => ((SnowModel)model).PostProcessing,
                    SetValue = (model, value) => ((SnowModel)model).PostProcessing = (float)value
                }
            },
            {
                nameof(IsLightning), new BoolProperty {
                    UniformName = "u_IsLightning",
                    GetValue = model => ((SnowModel)model).IsLightning,
                    SetValue = (model, value) => ((SnowModel)model).IsLightning = (bool)value
                }
            },
            {
                nameof(IsBlur), new BoolProperty {
                    UniformName = "u_IsBlur",
                    GetValue = model => ((SnowModel)model).IsBlur,
                    SetValue = (model, value) => ((SnowModel)model).IsBlur = (bool)value
                }
            },
            {
                nameof(ImagePath), new TextureProperty {
                    UniformName = "u_Texture",
                    GetValue = model => ((SnowModel)model).ImagePath,
                    SetValue = (model, value) => ((SnowModel)model).ImagePath = (string)value
                }
            }
        });
    }
}
