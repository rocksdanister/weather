using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.Common.Constants;

public static class AssetImageConstants
{
    public static IReadOnlyList<string> RainAssets { get; } = new List<string>{
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Snow", "0.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Rain", "0.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Rain", "1.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Rain", "2.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Rain", "3.jpg"),
        //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Snow", "3.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Rain", "4.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Snow", "4.jpg"),
        //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Rain", "5.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Rain", "6.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Rain", "7.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Rain", "8.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Rain", "9.jpg"),
    };

    public static IReadOnlyList<string> SnowAssets { get; } = new List<string>{
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Snow", "0.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Snow", "1.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Snow", "2.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Rain", "2.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Snow", "3.jpg"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Snow", "4.jpg"),
    };

    // Format: image.jpg, depth.jpg
    public static IReadOnlyList<string> DepthAssets { get; } = new List<string>{
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Depth", "0"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Depth", "1"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Depth", "2"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Backgrounds", "Depth", "3"),
    };
}
