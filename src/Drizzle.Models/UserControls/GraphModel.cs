using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.UserControls
{
    public partial class GraphModel : ObservableObject
    {
        [ObservableProperty]
        private int interval;

        [ObservableProperty]
        private float? maxValue;

        [ObservableProperty]
        private float? minValue;

        [ObservableProperty]
        private DateTime startTime;

        [ObservableProperty]
        private string valueFormat;

        [ObservableProperty]
        private float[] value;
    }
}
