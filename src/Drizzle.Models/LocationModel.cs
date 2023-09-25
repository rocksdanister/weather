using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models;

/// <summary>
/// Geo location data
/// </summary>
public partial class LocationModel : ObservableObject
{
    public LocationModel() { }

    public LocationModel(string name, float latitude, float longitude)
    {
        Name = name;
        Latitude = latitude;
        Longitude = longitude;
    }

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private float latitude;

    [ObservableProperty]
    private float longitude;
}
