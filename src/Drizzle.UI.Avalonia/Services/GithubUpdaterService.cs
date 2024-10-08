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
    private readonly string lastUpdateCheckedSettingKey = "UpdateLastChecked";
    private readonly string manifestFileName = "Update.json";
    private readonly ISystemInfoProvider systemInfo;
    private readonly IUserSettings userSettings;
    private readonly HttpClient httpClient;
    private readonly ILogger logger;

    public DateTime LastChecked { get; private set; }

    public GithubUpdaterService(IHttpClientFactory httpClientFactory,
        ISystemInfoProvider systemInfo,
        IUserSettings userSettings,
        ILogger<GithubUpdaterService> logger)
    {
        this.logger = logger;
        this.systemInfo = systemInfo;
        this.userSettings = userSettings;
        httpClient = httpClientFactory.CreateClient();

        LastChecked = userSettings.Get(lastUpdateCheckedSettingKey, DateTime.MinValue);
    }

    public async Task<AppUpdateStatus> CheckUpdateAsync()
    {
        try
        {
            var release = await GetLatestRelease("rocksdanister", "weather");
            var manifestUrl = GetAssetUrl(release, manifestFileName);
            var manifestString = await httpClient.GetStringAsync(manifestUrl);
            var manifest = JsonConvert.DeserializeObject<AppUpdateManifest>(manifestString);

            // Keep track to avoid checking frequently
            LastChecked = DateTime.UtcNow;
            userSettings.Set(lastUpdateCheckedSettingKey, LastChecked);

            if (systemInfo.AppVersion < manifest.ReleaseVersion)
                return AppUpdateStatus.available;
            else if (systemInfo.AppVersion > manifest.ReleaseVersion)
                return AppUpdateStatus.invalid;

            return AppUpdateStatus.uptodate;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check update.");
        }

        return AppUpdateStatus.error;
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
