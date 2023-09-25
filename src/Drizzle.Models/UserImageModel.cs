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
    public UserImageModel(string title, string image, bool isEditable)
    {
        this.Title = title;
        this.Image = image;
        this.IsEditable = isEditable;
    }

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string image;

    /// <summary>
    /// Is the image modifiable or is it preloaded asset
    /// </summary>
    [ObservableProperty]
    private bool isEditable;
}
