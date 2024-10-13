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

    public SnowModel(Uri shaderUri) : base(shaderUri, ShaderTypes.snow, scaleFactor: 0.75f, maxScaleFactor: 1f, mouseSpeed: 1.5f, mouseInertia: 0.08f)
    {
        InitializeUniformMappings();
    }

    public SnowModel() : base(null, ShaderTypes.snow, scaleFactor: 0.75f, maxScaleFactor: 1f, mouseSpeed: 1.5f, mouseInertia: 0.08f) 
    {
        InitializeUniformMappings();
    }

    public SnowModel(SnowModel properties) : base(properties.ShaderUri, ShaderTypes.snow, scaleFactor: 0.75f, maxScaleFactor: 1f, mouseSpeed: 1.5f, mouseInertia: 0.08f)
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
                    GetValue = model => ((SnowModel)model).Speed
                }
            },
            {
                nameof(Depth), new FloatProperty {
                    UniformName = "u_Depth",
                    GetValue = model => ((SnowModel)model).Depth
                }
            },
            {
                nameof(Width), new FloatProperty {
                    UniformName = "u_Width",
                    LerpSpeed = 0.01f,
                    GetValue = model => ((SnowModel)model).Width
                }
            },
            {
                nameof(Layers), new IntProperty {
                    UniformName = "u_Layers",
                    GetValue = model => ((SnowModel)model).Layers
                }
            },
            {
                nameof(PostProcessing), new FloatProperty {
                    UniformName = "u_PostProcessing",
                    LerpSpeed = 0.05f,
                    GetValue = model => ((SnowModel)model).PostProcessing
                }
            },
            {
                nameof(IsLightning), new BoolProperty {
                    UniformName = "u_IsLightning",
                    GetValue = model => ((SnowModel)model).IsLightning
                }
            },
            {
                nameof(IsBlur), new BoolProperty {
                    UniformName = "u_IsBlur",
                    GetValue = model => ((SnowModel)model).IsBlur
                }
            },
            {
                nameof(ImagePath), new TextureProperty {
                    UniformName = "u_Texture",
                    GetValue = model => ((SnowModel)model).ImagePath
                }
            }
        });
    }
}
