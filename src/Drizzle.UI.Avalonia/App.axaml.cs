using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Drizzle.Common.Constants;
using Drizzle.Common.Services;
using Drizzle.ML.DepthEstimate;
using Drizzle.Models.Enums;
using Drizzle.Models.Weather;
using Drizzle.UI.Avalonia.Services;
using Drizzle.UI.Avalonia.Views;
using Drizzle.UI.Shared.Factories;
using Drizzle.UI.Shared.Services;
using Drizzle.UI.Shared.ViewModels;
using Drizzle.Weather;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace Drizzle.UI.Avalonia;

public partial class App : Application
{
    public static IServiceProvider Services
    {
        get
        {
            IServiceProvider serviceProvider = ((App)Current)._serviceProvider;
            return serviceProvider ?? throw new InvalidOperationException("The service provider is not initialized");
        }
    }
    private readonly ILogger logger;
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        _serviceProvider = ConfigureServices();
        // Note: Application.Current is not ready.
        logger = _serviceProvider.GetRequiredService<ILogger<App>>();
        SetupUnhandledExceptionLogging();
        var systemInfo = _serviceProvider.GetRequiredService<ISystemInfoProvider>();
        logger.LogInformation("Application {AppName} v{AppVersion}", systemInfo.AppName, systemInfo.AppVersion);

        if (systemInfo.IsFirstRun)
        {
            // Update before viewModel initialization
            var regionInfo = new RegionInfo(CultureInfo.CurrentCulture.Name);
            var userSettings = _serviceProvider.GetRequiredService<IUserSettings>();
            switch (regionInfo.TwoLetterISORegionName)
            {
                case "US":
                    userSettings.SetAndSerialize(UserSettingsConstants.WeatherUnit, WeatherUnits.imperial);
                    break;
                case "GB":
                    userSettings.SetAndSerialize(UserSettingsConstants.WeatherUnit, WeatherUnits.hybrid);
                    break;
            }
            // Only Quality and Potato available in Avalonia for now.
            userSettings.SetAndSerialize(UserSettingsConstants.Performance, AppPerformance.quality);
        }
        else if (systemInfo.IsAppUpdated)
        {
            logger.LogInformation("App updated, performing maintenance..");
            // Clear cache incase any changes made to the impl
            _serviceProvider.GetRequiredService<ICacheService>().Clear();
        }
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        // Set display language.
        var userSettings = Services.GetRequiredService<IUserSettings>();
        var resources = Services.GetRequiredService<IResourceService>();
        if (userSettings.Get<bool>(UserSettingsConstants.UseSystemDefaultLanguage))
            resources.SetSystemDefaultCulture();
        else
            resources.SetCulture(userSettings.Get<string>(UserSettingsConstants.SelectedLanguageCode));

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<ShellViewModel>()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = Services.GetRequiredService<ShellViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private IServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            // Singleton
            .AddSingleton<ShellViewModel>()
            .AddSingleton<INavigator, Navigator>()
            .AddSingleton<IDialogService, DialogService>()
            .AddSingleton<IUserSettings, LocalSettings>()
            .AddSingleton<IAppUpdaterService, GithubUpdaterService>()
            .AddSingleton<IResourceService, ResourceService>()
            .AddSingleton<ISystemInfoProvider, SystemInfoProvider>()
            .AddSingleton<IGeolocationService, GeolocationService>()
                .AddSingleton<ICacheService, DiskCacheService>((e) => new DiskCacheService(
                e.GetRequiredService<IHttpClientFactory>(),
                e.GetRequiredService<IFileService>().CachePath,
                TimeSpan.FromHours(1)))
            .AddSingleton<IDepthEstimate, MiDaS>()
            .AddSingleton<ISoundService, SoundService>()
            .AddSingleton<IAssetReader, AssetReader>()
            .AddSingleton<IFileService, FileService>()
            .AddSingleton<IWeatherClient, OpenMeteoWeatherClient>()
            .AddSingleton<IWeatherClient>((e) => new OpenWeatherMapWeatherClient(
                e.GetRequiredService<IHttpClientFactory>(),
                e.GetRequiredService<ICacheService>(),
                e.GetRequiredService<IUserSettings>().Get<string>(UserSettingsConstants.OpenWeatherMapKey)))
            .AddSingleton<IWeatherClient>((e) => new QweatherWeatherClient(
                e.GetRequiredService<IHttpClientFactory>(),
                e.GetRequiredService<ICacheService>(),
                e.GetRequiredService<IUserSettings>().Get<string>(UserSettingsConstants.QweatherApiKey)
                ))
            // Transient
            .AddTransient<HelpViewModel>()
            .AddTransient<AboutViewModel>()
            .AddTransient<SettingsViewModel>()
            .AddTransient<ScreensaverViewModel>()
            .AddTransient<DepthEstimateViewModel>()
            .AddTransient<IShaderViewModelFactory, ShaderViewModelFactory>()
            .AddTransient<IWeatherViewModelFactory, WeatherViewModelFactory>()
            .AddTransient<IWeatherClientFactory, WeatherClientFactory>()
            .AddTransient<IDownloadService, HttpDownloadService>()
            .AddTransient<ILauncherService, LauncherService>()
            .AddTransient<INLogConfigFactory, NLogConfigFactory>()
            // Ref: https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            .AddHttpClient()
            // Remove HttpClientFactory logging
            .RemoveAll<IHttpMessageHandlerBuilderFilter>()
            // Deferred logging initialization
            .AddSingleton(provider =>
            {
                var configFactory = provider.GetRequiredService<INLogConfigFactory>();
                var logFolderPath = provider.GetRequiredService<IFileService>().LogFolderPath;

                return LoggerFactory.Create(builder =>
                {
                    builder.ClearProviders();
                    // Ref: https://github.com/NLog/NLog.Extensions.Logging/issues/389
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddNLog(configFactory.Create(logFolderPath));
                });
            }).BuildServiceProvider();
    }

    private void SetupUnhandledExceptionLogging()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            LogUnhandledException((Exception)e.ExceptionObject);

        TaskScheduler.UnobservedTaskException += (s, e) =>
            LogUnhandledException(e.Exception);

        Dispatcher.UIThread.UnhandledException += (s, e) =>
            LogUnhandledException(e.Exception);
    }

    private void LogUnhandledException(Exception? ex)
    {
        if (ex is not null)
            logger.LogError(ex, "An unhandled exception occurred.");
    }
}
