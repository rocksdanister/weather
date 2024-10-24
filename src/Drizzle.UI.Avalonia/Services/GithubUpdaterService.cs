using Avalonia.Threading;
using Drizzle.Common.Services;
using Drizzle.Models;
using Drizzle.Models.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Octokit;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Drizzle.UI.Avalonia.Services;

public class GithubUpdaterService : IAppUpdaterService
{
    private readonly string lastUpdateCheckedUtcTimeSettingKey = "UpdateLastCheckedUtcTime";
    private readonly string lastUpdateCheckedStatusKey = "UpdateLastCheckedStatus";
    private readonly TimeSpan updateCheckInterval = TimeSpan.FromHours(6);
    private readonly string manifestFileName = "Update.Linux.json";
    private readonly DispatcherTimer dispatcherTimer = new();

    private readonly ISystemInfoProvider systemInfo;
    private readonly IUserSettings userSettings;
    private readonly HttpClient httpClient;
    private readonly ILogger logger;

    public event EventHandler<AppUpdateStatus> UpdateChecked;
    public DateTime LastCheckedTime { get; private set; }
    public AppUpdateStatus LastCheckedStatus { get; private set; }

    public GithubUpdaterService(IHttpClientFactory httpClientFactory,
        ISystemInfoProvider systemInfo,
        IUserSettings userSettings,
        ILogger<GithubUpdaterService> logger)
    {
        this.logger = logger;
        this.systemInfo = systemInfo;
        this.userSettings = userSettings;
        httpClient = httpClientFactory.CreateClient();

        LastCheckedTime = userSettings.Get(lastUpdateCheckedUtcTimeSettingKey, DateTime.MinValue);
        // If the application just updated we do not know if it is latest update, assume not checked.
        LastCheckedStatus = systemInfo.IsAppUpdated ? 
            AppUpdateStatus.notchecked : userSettings.GetAndDeserialize(lastUpdateCheckedStatusKey, AppUpdateStatus.notchecked);

        dispatcherTimer.Tick += DispatcherTimer_Tick;
        dispatcherTimer.Interval = new TimeSpan(0, 30, 0);
    }

    public void Start()
    {
        dispatcherTimer.Start();
        // Start does not call once initially.
        DispatcherTimer_Tick(this, EventArgs.Empty);
    }

    public async Task<AppUpdateStatus> CheckUpdateAsync()
    {
        AppUpdateStatus result;
        try
        {
            var release = await GetLatestRelease("rocksdanister", "weather");
            var manifestUrl = GetAssetUrl(release, manifestFileName);
            var manifestString = await httpClient.GetStringAsync(manifestUrl);
            var manifest = JsonConvert.DeserializeObject<AppUpdateManifest>(manifestString);

            // Keep track to avoid checking frequently
            LastCheckedTime = DateTime.UtcNow;
            userSettings.Set(lastUpdateCheckedUtcTimeSettingKey, LastCheckedTime);

            if (systemInfo.AppVersion < manifest.ReleaseVersion)
                result = AppUpdateStatus.available;
            else if (systemInfo.AppVersion > manifest.ReleaseVersion)
                result = AppUpdateStatus.invalid;
            else
                result = AppUpdateStatus.uptodate;
        }
        catch (Exception ex)
        {
            result = AppUpdateStatus.error;
            logger.LogError(ex, "Failed to check update.");
        }
        LastCheckedStatus = result;
        userSettings.SetAndSerialize(lastUpdateCheckedStatusKey, LastCheckedStatus);
        UpdateChecked?.Invoke(this, LastCheckedStatus);

        return result;
    }

    private async void DispatcherTimer_Tick(object? sender, EventArgs e)
    {
        if ((DateTime.UtcNow - LastCheckedTime) > updateCheckInterval)
            await CheckUpdateAsync();
    }

    private static async Task<Release> GetLatestRelease(string userName, string repositoryName)
    {
        var client = new GitHubClient(new ProductHeaderValue(repositoryName));
        var releases = await client.Repository.Release.GetAll(userName, repositoryName);
        var latest = releases[0];

        return latest;
    }

    private static string GetAssetUrl(Release release, string assetName)
    {
        return release.Assets.FirstOrDefault(x => x.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase))?.BrowserDownloadUrl 
            ?? throw new FileNotFoundException("Manifest missing.");
    }
}
