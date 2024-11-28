using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models.Enums;
using Drizzle.Models.Shaders.Uniform;
using System;
using System.Collections.Generic;

namespace Drizzle.Models.Shaders
{
    public partial class TunnelModel : ShaderModel
    {
        [ObservableProperty]
        private float speed = 1f;

        [ObservableProperty]
        private bool isSquare = false;

        public string? ImagePath { get; set; }

        public TunnelModel(Uri shaderUri, TunnelModel properties) : base(
           shaderUri ?? properties?.ShaderUri,
           ShaderTypes.tunnel,
           scaleFactor: 1f,
           maxScaleFactor: 1f,
           mouseSpeed: 1f,
           mouseInertia: 1f)
        {
            if (properties != null)
            {
                this.ImagePath = properties.ImagePath;
                this.IsSquare = properties.IsSquare;
                this.speed = properties.Speed;
                this.IsDaytime = properties.IsDaytime;
            }

            InitializeUniformMappings();
        }

        public TunnelModel(Uri shaderUri) : this(shaderUri, null) { }

        public TunnelModel() : this(null, null) { }

        public TunnelModel(TunnelModel properties) : this(null, properties) { }

        protected override void InitializeUniformMappings()
        {
            base.InitializeUniformMappings();

            AddUniformMappings(new Dictionary<string, UniformProperty>
            {
                {
                    nameof(Speed), new FloatProperty {
                        UniformName = "u_Speed",
                        GetValue = model => ((TunnelModel)model).Speed,
                        SetValue = (model, value) => ((TunnelModel)model).Speed = (float)value
                    }
                },
                {
                    nameof(IsSquare), new BoolProperty {
                        UniformName = "u_IsSquare",
                        GetValue = model => ((TunnelModel)model).IsSquare,
                        SetValue = (model, value) => ((TunnelModel)model).IsSquare = (bool)value
                    }
                },
                {
                    nameof(ImagePath), new TextureProperty {
                        UniformName = "u_Texture",
                        WrapMode = TextureWrapMode.mirror,
                        GetValue = model => ((TunnelModel)model).ImagePath,
                        SetValue = (model, value) => ((TunnelModel)model).ImagePath = (string)value
                    }
                }
            });
        }
    }
}
