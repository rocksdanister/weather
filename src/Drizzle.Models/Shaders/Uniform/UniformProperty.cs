using System;
using Drizzle.Models.Enums;

namespace Drizzle.Models.Shaders.Uniform;

public class UniformProperty
{
    public UniformProperty(UniformTypes type)
    {
        this.UniformType = type;
    }

    public UniformTypes UniformType { get; }
    public string UniformName { get; set; }
    public Func<ShaderModel, object> GetValue { get; set; }
}
