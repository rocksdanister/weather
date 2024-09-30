using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Drizzle.Common.Constants;
using Drizzle.Common.Helpers;
using Drizzle.Common.Services;
using Drizzle.ML.DepthEstimate;
using Drizzle.UI.Avalonia.Constants;
using Drizzle.UI.Avalonia.Helpers;
using Drizzle.UI.Avalonia.Services;
using Drizzle.UI.Avalonia.Views;
using Drizzle.UI.Shared.Factories;
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
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        _serviceProvider = ConfigureServices();
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

        //ResourceUtil.SetCulture("en-US");
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
            //.AddSingleton<IAppUpdaterService, AppUpdaterService>()
            .AddSingleton<ISystemInfoProvider, SystemInfoProvider>()
            .AddSingleton<IGeolocationService, GeolocationService>()
                .AddSingleton<ICacheService, DiskCacheService>((e) => new DiskCacheService(
                e.GetRequiredService<IHttpClientFactory>(),
                PathConstants.CacheDir,
                TimeSpan.FromHours(1)))
            .AddSingleton<IDepthEstimate, MiDaS>()
            .AddSingleton<ISoundService, SoundService>()
            .AddSingleton<IAssetReader, AssetReader>()
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
            .AddTransient<IDownloadUtil, HttpDownloadUtil>()
            .AddTransient<IBrowserUtil, BrowserUtil>()
            // https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            .AddHttpClient()
            // Remove HttpClientFactory logging
            .RemoveAll<IHttpMessageHandlerBuilderFilter>()
            // TODO: Use proper MacOS, Linux logfile location.
            .AddLogging(loggingBuilder =>
            {
                // Configure Logging with NLog
                loggingBuilder.ClearProviders();
                // https://github.com/NLog/NLog.Extensions.Logging/issues/389
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddNLog("Nlog.config");
            })
            .BuildServiceProvider();
    }
}
