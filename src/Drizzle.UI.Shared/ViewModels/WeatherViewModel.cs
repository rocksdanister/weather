using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Common;
using Drizzle.Models;
using Drizzle.Models.Weather;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drizzle.Weather.Helpers;

namespace Drizzle.UI.UWP.ViewModels
{
    public sealed partial class WeatherViewModel : ObservableObject
    {
        [ObservableProperty]
        private LocationModel location;

        [ObservableProperty]
        private DateTime fetchTime;

        [ObservableProperty]
        private string timeZone;

        [ObservableProperty]
        private float minTemp;

        [ObservableProperty]
        private float maxTemp;

        [ObservableProperty]
        private ObservableCollection<WeatherModel> daily = new();

        // For location selector
        [ObservableProperty]
        WeatherModel today;
    }
}
