using Drizzle.Models.Enums;
using Drizzle.Models.Shaders;
using Drizzle.UI.Shared.ViewModels;
using System;

#if WINDOWS_UWP
using Drizzle.UI.Shaders.Runners;
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
            ShaderTypes.clouds => new CloudsRunner(() => shaderVm.Model as CloudsModel),
            ShaderTypes.rain => new RainRunner(() => shaderVm.Model as RainModel),
            ShaderTypes.snow => new SnowRunner(() => shaderVm.Model as SnowModel),
            ShaderTypes.depth => new DepthRunner(() => shaderVm.Model as DepthModel),
            ShaderTypes.fog => new WindRunner(() => shaderVm.Model as WindModel),
            ShaderTypes.tunnel => new TunnelRunner(() => shaderVm.Model as TunnelModel),
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
