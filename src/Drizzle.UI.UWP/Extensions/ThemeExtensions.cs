using Drizzle.Models.Enums;
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
