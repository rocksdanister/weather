using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Models;
using System;
using System.Collections.ObjectModel;

namespace Drizzle.UI.Shared.ViewModels;

public sealed partial class WeatherViewModel : ObservableObject
{
    [ObservableProperty]
    private int sortOrder;

    [ObservableProperty]
    private LocationModel location;

    [ObservableProperty]
    private DateTime fetchTime;

    [ObservableProperty]
    private string timeZone;

    [ObservableProperty]
    private ObservableCollection<WeatherModel> daily = new();

    // For location selector
    [ObservableProperty]
    private WeatherModel today;

    [ObservableProperty]
    private int forecastInterval;

    [ObservableProperty]
    private int forecastAQInterval;
}
