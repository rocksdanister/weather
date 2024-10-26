using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Drizzle.Models.Shaders.Uniform;

namespace Drizzle.UI.UWP.Helpers;

public class UniformItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate SliderTemplate { get; set; }
    public DataTemplate ColorPickerTemplate { get; set; }
    public DataTemplate CheckboxTemplate { get; set; }
    public DataTemplate ImagePickerTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is UniformProperty uniform)
        {
            return uniform.UniformType switch
            {
                Models.Enums.UniformType.int_ => SliderTemplate,
                Models.Enums.UniformType.float_ => SliderTemplate,
                Models.Enums.UniformType.bool_ => CheckboxTemplate,
                Models.Enums.UniformType.color => ColorPickerTemplate,
                Models.Enums.UniformType.textureUri => ImagePickerTemplate,
                _ => throw new NotImplementedException($"Uniform type '{uniform.UniformType}' is not supported."),
            };
        }
        throw new NotImplementedException();
    }
}
