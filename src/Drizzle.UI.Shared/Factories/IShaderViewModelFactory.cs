using Drizzle.Models.Enums;
using Drizzle.UI.Shared.ViewModels;

namespace Drizzle.UI.Shared.Factories;

public interface IShaderViewModelFactory
{
    public ShaderViewModel Create(ShaderTypes shaderType);
}
