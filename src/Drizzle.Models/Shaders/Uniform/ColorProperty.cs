using Drizzle.Models.Enums;

namespace Drizzle.Models.Shaders.Uniform;

public class ColorProperty : UniformProperty
{
    public ColorProperty() : base(UniformType.color)
    {
    }
}
