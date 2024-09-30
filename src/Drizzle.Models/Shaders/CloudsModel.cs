using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Enums;
using Drizzle.Models.Shaders.Uniform;
using System;
using System.Collections.Generic;

namespace Drizzle.Models.Shaders;

public partial class CloudsModel : ShaderModel
{
    [ObservableProperty]
    private float speed = 0.25f;

    [ObservableProperty]
    private float scale = 0.61f;

    [ObservableProperty]
    private int iterations = 5;

    [ObservableProperty]
    private bool isDayNightShift = true;

    public CloudsModel(Uri shaderUri) : base(shaderUri, ShaderTypes.clouds, scaleFactor: 0.2f, maxScaleFactor: 0.4f, mouseSpeed: 1.5f, mouseInertia: 0.08f)
    {
        InitializeUniformMappings();
    }

    public CloudsModel() : base(null, ShaderTypes.clouds, scaleFactor: 0.2f, maxScaleFactor: 0.4f, mouseSpeed: 1.5f, mouseInertia: 0.08f)
    {
        InitializeUniformMappings();
    }

    public CloudsModel(CloudsModel properties) : base(null, ShaderTypes.clouds, scaleFactor: 0.2f, maxScaleFactor: 0.4f, mouseSpeed: 1.5f, mouseInertia: 0.08f)
    {
        this.Speed = properties.Speed;
        this.Scale = properties.Scale;
        this.Mouse = properties.Mouse;
        this.Saturation = properties.Saturation;
        this.IsDayNightShift = properties.IsDayNightShift;
        this.IsDaytime = properties.IsDaytime;

        InitializeUniformMappings();
    }

    protected override void InitializeUniformMappings()
    {
        uniformMappings = new Dictionary<string, UniformProperty>
        {
            {
                nameof(Scale), new FloatProperty {
                    UniformName = "u_Scale",
                    GetValue = model => ((CloudsModel)model).Scale
                }
            },
            {
                nameof(Iterations), new IntProperty {
                    UniformName = "u_Iterations",
                    GetValue = model => ((CloudsModel)model).Iterations
                }
            },
            {
                nameof(Speed), new FloatProperty {
                    UniformName = "u_Speed",
                    GetValue = model => ((CloudsModel)model).Speed
                }
            },
            {
                nameof(IsDaytime), new BoolProperty {
                    UniformName = "u_IsDaytime",
                    GetValue = model => ((CloudsModel)model).IsDaytime
                }
            },
            {
                nameof(IsDayNightShift), new BoolProperty {
                    UniformName = "u_IsDayNightShift",
                    GetValue = model => ((CloudsModel)model).IsDayNightShift
                }
            }
        };
    }
}
