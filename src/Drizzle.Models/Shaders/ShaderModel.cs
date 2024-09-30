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

    public bool IsDaytime { get; set; }

    public IReadOnlyDictionary<string, UniformProperty> UniformMappings => uniformMappings;

    protected Dictionary<string, UniformProperty> uniformMappings;

    protected ShaderModel(Uri? shaderUri,
        ShaderTypes type,
        float scaleFactor,
        float maxScaleFactor,
        float mouseSpeed,
        float mouseInertia)
    {
        this.Type = type;
        this.ShaderUri = shaderUri;
        this.ScaleFactor = scaleFactor;
        this.MaxScaleFactor = maxScaleFactor;
        this.MouseInertia = mouseInertia;
        this.MouseSpeed = mouseSpeed;
    }

    protected abstract void InitializeUniformMappings();
}
