using Drizzle.Common.Services;
using Drizzle.UI.Avalonia.Views;

namespace Drizzle.UI.Avalonia.Services;

public class WindowService : IWindowService
{
    public void ShowAboutWindow()
    {
        var window = new AboutWindow();
        window.Show();
    }

    public void ShowHelpWindow()
    {
        var window = new HelpWindow();
        window.Show();
    }
}
