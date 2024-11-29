using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Enums;
using Drizzle.Models.Shaders.Uniform;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Drizzle.Models.Shaders;

public abstract class ShaderModel : ObservableObject
{
    public ShaderTypes Type { get; }

    // Example: (MouseSpeed * MousePos - Offset) * MouseInertia
    public Vector4 Mouse { get; set; } = Vector4.Zero;

    public float MouseSpeed { get; set; } = 1f;

    public float MouseInertia { get; set; } = 1f;

    public float Brightness { get; set; } = 1f;

    public float Saturation { get; set; } = 1f;

    public float TimeMultiplier { get; set; } = 1f;

    public Uri? ShaderUri { get; set; }

    public float ScaleFactor { get; }

    public float MaxScaleFactor { get; }

    public float MaxFrameRate { get; }

    public bool IsDaytime { get; set; }

    public IReadOnlyDictionary<string, UniformProperty> UniformMappings => uniformMappings;

    private readonly Dictionary<string, UniformProperty> uniformMappings = [];

    protected ShaderModel(Uri? shaderUri,
        ShaderTypes type,
        float scaleFactor,
        float maxScaleFactor,
        float mouseSpeed,
        float mouseInertia,
        float maxFrameRate = 60)
    {
        this.Type = type;
        this.ShaderUri = shaderUri;
        this.ScaleFactor = scaleFactor;
        this.MaxScaleFactor = maxScaleFactor;
        this.MouseInertia = mouseInertia;
        this.MouseSpeed = mouseSpeed;
        this.MaxFrameRate = maxFrameRate;
    }

    protected virtual void InitializeUniformMappings()
    {
        // ShaderModel defaults
        AddUniformMappings(new Dictionary<string, UniformProperty>
        {
            {
                nameof(Brightness), new FloatProperty
                {
                    IsDefault = true,
                    UniformName = "u_Brightness",
                    LerpSpeed = 0.02f,
                    GetValue = model => model.Brightness,
                    SetValue = (model, value) => model.Brightness = (float)value
                }
            },
            {
                nameof(Saturation), new FloatProperty
                {
                    IsDefault = true,
                    UniformName = "u_Saturation",
                    LerpSpeed = 0.01f,
                    GetValue = model => model.Saturation,
                    SetValue = (model, value) => model.Saturation = (float)value
                }
            }
        });
    }

    protected void AddUniformMappings(Dictionary<string, UniformProperty> newMappings)
    {
        foreach (var mapping in newMappings)
        {
            uniformMappings[mapping.Key] = mapping.Value;
        }
    }
}
