using Drizzle.Models.Enums;

namespace Drizzle.Models.Shaders.Uniform;

public class FloatProperty : UniformProperty
{
    public float LerpSpeed { get; set; }

    public FloatProperty() : base(UniformType.float_)
    {
    }
}
