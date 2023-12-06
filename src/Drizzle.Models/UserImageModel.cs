using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models;

/// <summary>
/// Screensaver customise image browser template
/// </summary>
public partial class UserImageModel : ObservableObject
{
    public UserImageModel(string title, string image, DateTime time, bool isEditable)
    {
        this.Title = title;
        this.Image = image;
        this.time = time;
        this.IsEditable = isEditable;
    }

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string image;

    [ObservableProperty]
    private DateTime time;

    /// <summary>
    /// Is the image modifiable or is it preloaded asset
    /// </summary>
    [ObservableProperty]
    private bool isEditable;
}
