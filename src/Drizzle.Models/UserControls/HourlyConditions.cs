using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.UserControls
{
    /// <summary>
    /// Daily graph hourly data
    /// </summary>
    public partial class HourlyConditions : ObservableObject
    {
        [ObservableProperty]
        private int weatherCode;

        [ObservableProperty]
        private bool isDaytime;

        [ObservableProperty]
        private float left;

        [ObservableProperty]
        private float top;

        public HourlyConditions(int weatherCode, bool isDaytime, float left, float top)
        {
            this.WeatherCode = weatherCode;
            this.IsDaytime = isDaytime;
            this.Left = left;
            this.Top = top;
        }
    }
}
