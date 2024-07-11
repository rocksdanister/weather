using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Enums;

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

    public CloudsModel() : base(ShaderTypes.clouds) { }

    public CloudsModel(CloudsModel properties) : base(ShaderTypes.clouds)
    {
        this.Speed = properties.Speed;
        this.Scale = properties.Scale;
        this.Mouse = properties.Mouse;
        this.Saturation = properties.Saturation;
        this.IsDayNightShift = properties.IsDayNightShift;
        this.IsDaytime = properties.IsDaytime;
    }
}
