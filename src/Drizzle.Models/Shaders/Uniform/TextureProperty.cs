using Drizzle.Models.Enums;

namespace Drizzle.Models.Shaders.Uniform;

public class TextureProperty : UniformProperty
{
    public TextureWrapMode WrapMode { get; set; } = TextureWrapMode.clamp;

    public TextureProperty() : base(UniformType.textureUri)
    {
    }
}
