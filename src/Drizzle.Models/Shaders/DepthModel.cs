using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Enums;
using Drizzle.Models.Shaders.Uniform;
using System;
using System.Collections.Generic;

namespace Drizzle.Models.Shaders;

public partial class DepthModel : ShaderModel
{
    [ObservableProperty]
    private float intensityX = 0.5f;

    [ObservableProperty]
    private float intensityY = 0.5f;

    [ObservableProperty]
    private bool isBlur = true;

    public string? ImagePath { get; set; }

    public string? DepthPath { get; set; }

    public DepthModel(Uri shaderUri) : base(shaderUri, ShaderTypes.depth, scaleFactor: 1f, maxScaleFactor: 1f, mouseSpeed: -0.075f, mouseInertia: 0.08f)
    {
        InitializeUniformMappings();
    }

    public DepthModel() : base(null, ShaderTypes.depth, scaleFactor: 1f, maxScaleFactor: 1f, mouseSpeed: -0.075f, mouseInertia: 0.08f)
    {
        InitializeUniformMappings();
    }

    public DepthModel(DepthModel properties) : base(null, ShaderTypes.depth, scaleFactor: 1f, maxScaleFactor: 1f, mouseSpeed: -0.075f, mouseInertia: 0.08f)
    {
        this.Mouse = properties.Mouse;
        this.IntensityX = properties.IntensityX;
        this.intensityY = properties.IntensityY;
        this.ImagePath = properties.ImagePath;
        this.DepthPath = properties.DepthPath;
        this.Saturation = properties.Saturation;
        this.IsDaytime = properties.IsDaytime;

        InitializeUniformMappings();
    }

    protected override void InitializeUniformMappings()
    {
        uniformMappings = new Dictionary<string, UniformProperty>
        {
            {
                nameof(IntensityX), new FloatProperty {
                    UniformName = "u_IntensityX",
                    GetValue = model => ((DepthModel)model).IntensityX
                }
            },
            {
                nameof(IntensityY), new FloatProperty {
                    UniformName = "u_IntensityY",
                    GetValue = model => ((DepthModel)model).IntensityY
                }
            },
            {
                nameof(IsBlur), new BoolProperty {
                    UniformName = "u_IsBlur",
                    GetValue = model => ((DepthModel)model).IsBlur
                }
            },
            {
                nameof(ImagePath), new TextureProperty {
                    UniformName = "u_Texture",
                    GetValue = model => ((DepthModel)model).ImagePath
                }
            },
            {
                nameof(DepthPath), new TextureProperty {
                    UniformName = "u_DepthTexture",
                    GetValue = model => ((DepthModel)model).DepthPath
                }
            }
        };
    }
}
