using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Common;
using Drizzle.UI.Shared.Shaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.UI.Shared.Shaders.Models;

public partial class CloudsModel : BaseModel
{
    [ObservableProperty]
    private float speed = 0.25f;

    [ObservableProperty]
    private float scale = 0.61f;

    [ObservableProperty]
    private int iterations = 5;

    [ObservableProperty]
    private bool isDayNightShift = true;

    public static float DefaultBrightness { get; } = 1f;

    public static float DefaultSaturation { get; } = 1f;

    public CloudsModel() : base(ShaderTypes.clouds, DefaultBrightness, DefaultSaturation) { }

    public CloudsModel(CloudsModel properties) : base(ShaderTypes.clouds, DefaultBrightness, DefaultSaturation)
    {
        this.Speed = properties.Speed;
        this.Scale = properties.Scale;
        this.Mouse = properties.Mouse;
        this.Saturation = properties.Saturation;
        this.IsDayNightShift = properties.IsDayNightShift;
    }
}
