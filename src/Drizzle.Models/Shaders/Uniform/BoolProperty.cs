using Drizzle.Models.Enums;

namespace Drizzle.Models.Shaders.Uniform;

public class BoolProperty : UniformProperty
{
    public BoolProperty() : base(UniformTypes.bool_)
    {
    }
}
