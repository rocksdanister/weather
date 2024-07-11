using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Common;
using Drizzle.Models.Enums;
using Drizzle.UI.Shared.Shaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.UI.Shared.Shaders.Models;

public partial class SnowModel : BaseModel
{
    [ObservableProperty]
    private float speed = 0.6f;

    [ObservableProperty]
    private float moveSpeed = 1.5f;

    [ObservableProperty]
    private float inertia = 0.08f;

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

    public string ImagePath { get; set; }

    public SnowModel() : base(ShaderTypes.snow) { }

    public SnowModel(SnowModel properties) : base(ShaderTypes.snow)
    {
        this.Speed = properties.Speed;
        this.MoveSpeed = properties.MoveSpeed;
        this.TimeMultiplier = properties.TimeMultiplier;
        this.Inertia = properties.Inertia;
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
}
