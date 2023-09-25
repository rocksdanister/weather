using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.Common.Services;

public interface IDialogService
{
    public Task ShowSettingsDialogAsync();

    public Task ShowAboutDialogAsync();

    public Task ShowHelpDialogAsync();

    public Task<string> ShowDepthCreationDialogAsync();
}
