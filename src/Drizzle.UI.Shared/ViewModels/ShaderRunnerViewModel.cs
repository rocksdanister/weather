using Drizzle.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using ComputeSharp.Uwp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.UI.UWP.ViewModels
{
    public sealed partial class ShaderRunnerViewModel : ObservableObject
    {
        public ShaderRunnerViewModel(IShaderRunner shaderRunner,
            ShaderTypes type,
            float scaleFactor,
            float maxScaleFactor)
        {
            this.ShaderRunner = shaderRunner;
            this.ShaderType = type;
            this.ScaleFactor = scaleFactor;
            this.MaxScaleFactor = maxScaleFactor;
        }

        public IShaderRunner ShaderRunner { get; }

        public ShaderTypes ShaderType { get; }

        public float ScaleFactor { get; }

        public float MaxScaleFactor { get; }

        [ObservableProperty]
        private bool isSelected;
    }
}
