using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models;

/// <summary>
/// Screensaver weather options
/// </summary>
public partial class ScreensaverModel : ObservableObject
{
    public ScreensaverModel(int weatherCode)
    {
        this.WeatherCode = weatherCode;
    }

    [ObservableProperty]
    private int weatherCode;
}
