using Drizzle.Models.Enums;
using Drizzle.Models.Shaders;
using Drizzle.UI.Shared.ViewModels;
using System;

#if WINDOWS_UWP
using DX12 = Drizzle.UI.Shaders.DX12.Runners;
using D2D1 = Drizzle.UI.UWP.Shaders.D2D1.Runners;
#endif

namespace Drizzle.UI.Shared.Factories;

public class ShaderViewModelFactory : IShaderViewModelFactory
{
    public ShaderViewModel Create(ShaderTypes shaderType)
    {
        var shaderVm = new ShaderViewModel();
#if WINDOWS_UWP
        shaderVm.Model = shaderType switch
        {
            ShaderTypes.clouds => new CloudsModel(),
            ShaderTypes.rain => new RainModel(),
            ShaderTypes.snow => new SnowModel(),
            ShaderTypes.depth => new DepthModel(),
            ShaderTypes.fog => new WindModel(),
            ShaderTypes.tunnel => new TunnelModel(),
            _ => throw new NotImplementedException(),
        };
        shaderVm.Runner = shaderType switch
        {
            ShaderTypes.clouds => new DX12.CloudsRunner(() => shaderVm.Model as CloudsModel),
            ShaderTypes.rain => new DX12.RainRunner(() => shaderVm.Model as RainModel),
            ShaderTypes.snow => new DX12.SnowRunner(() => shaderVm.Model as SnowModel),
            ShaderTypes.depth => new DX12.DepthRunner(() => shaderVm.Model as DepthModel),
            ShaderTypes.fog => new DX12.WindRunner(() => shaderVm.Model as WindModel),
            ShaderTypes.tunnel => new DX12.TunnelRunner(() => shaderVm.Model as TunnelModel),
            _ => throw new NotImplementedException(),
        };
        shaderVm.D2D1ShaderRunner = shaderType switch
        {
            ShaderTypes.clouds => new D2D1.CloudsRunner(() => shaderVm.Model as CloudsModel),
            ShaderTypes.rain => new D2D1.RainRunner(() => shaderVm.Model as RainModel),
            ShaderTypes.snow => new D2D1.SnowRunner(() => shaderVm.Model as SnowModel),
            ShaderTypes.depth => new D2D1.DepthRunner(() => shaderVm.Model as DepthModel),
            ShaderTypes.fog => new D2D1.WindRunner(() => shaderVm.Model as WindModel),
            ShaderTypes.tunnel => new D2D1.TunnelRunner(() => shaderVm.Model as TunnelModel),
            _ => throw new NotImplementedException(),
        };
#else
        shaderVm.Model = shaderType switch
        {
            ShaderTypes.clouds => new CloudsModel(new Uri("avares://Drizzle.UI.Avalonia/Shaders/Clouds.sksl")),
            ShaderTypes.rain => new RainModel(new Uri("avares://Drizzle.UI.Avalonia/Shaders/Rain.sksl")),
            ShaderTypes.snow => new SnowModel(new Uri("avares://Drizzle.UI.Avalonia/Shaders/Snow.sksl")),
            ShaderTypes.depth => new DepthModel(new Uri("avares://Drizzle.UI.Avalonia/Shaders/Depth.sksl")),
            ShaderTypes.fog => new WindModel(new Uri("avares://Drizzle.UI.Avalonia/Shaders/Wind.sksl")),
            ShaderTypes.tunnel => new TunnelModel(new Uri("avares://Drizzle.UI.Avalonia/Shaders/Tunnel.sksl")),
            _ => throw new NotImplementedException(),
        };
#endif
        return shaderVm;
    }
}
