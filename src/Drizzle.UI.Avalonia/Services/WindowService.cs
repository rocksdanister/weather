using Drizzle.Common.Services;
using Drizzle.UI.Avalonia.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.UI.Avalonia.Services;

public class WindowService : IWindowService
{
    public void ShowAboutWindow()
    {
        var window = new AboutWindow();
        window.Show();
    }
}
