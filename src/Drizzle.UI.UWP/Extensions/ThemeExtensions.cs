using Drizzle.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Drizzle.UI.UWP.Extensions;

public static class ThemeExtensions
{
    public static ElementTheme ToTheme(this AppTheme theme)
    {
        return theme switch
        {
            AppTheme.light => ElementTheme.Light,
            AppTheme.dark => ElementTheme.Dark,
            AppTheme.auto => ElementTheme.Default,
            _ => ElementTheme.Default,
        };
    }
}
