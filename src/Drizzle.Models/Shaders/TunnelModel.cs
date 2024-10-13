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

        public TunnelModel(Uri shaderUri) : base(shaderUri, ShaderTypes.tunnel, scaleFactor: 1f, maxScaleFactor: 1f, mouseSpeed: 1f, mouseInertia: 1f)
        {
            InitializeUniformMappings();
        }

        public TunnelModel() : base(null, ShaderTypes.tunnel, scaleFactor: 1f, maxScaleFactor: 1f, mouseSpeed: 1f, mouseInertia: 1f)
        {
            InitializeUniformMappings();
        }

        public TunnelModel(TunnelModel properties) : base(properties.ShaderUri, ShaderTypes.tunnel, scaleFactor: 1f, maxScaleFactor: 1f, mouseSpeed: 1f, mouseInertia: 1f)
        {
            this.ImagePath = properties.ImagePath;
            this.IsSquare = properties.IsSquare;
            this.speed = properties.Speed;
            this.IsDaytime = properties.IsDaytime;

            InitializeUniformMappings();
        }

        protected override void InitializeUniformMappings()
        {
            uniformMappings = new Dictionary<string, UniformProperty>
            {
                {
                    nameof(Speed), new FloatProperty {
                        UniformName = "u_Speed",
                        GetValue = model => ((TunnelModel)model).Speed
                    }
                },
                {
                    nameof(IsSquare), new BoolProperty {
                        UniformName = "u_IsSquare",
                        GetValue = model => ((TunnelModel)model).IsSquare
                    }
                },
                {
                    nameof(ImagePath), new TextureProperty {
                        UniformName = "u_Texture",
                        WrapMode = TextureWrapMode.mirror,
                        GetValue = model => ((TunnelModel)model).ImagePath
                    }
                }
            };
        }
    }
}
