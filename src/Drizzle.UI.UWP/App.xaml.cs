using Drizzle.Common.Constants;
using Drizzle.Common.Helpers;
using Drizzle.Common.Services;
using Drizzle.ML.DepthEstimate;
using Drizzle.Models.Enums;
using Drizzle.Models.Weather;
using Drizzle.UI.UWP.Extensions;
using Drizzle.UI.UWP.Factories;
using Drizzle.UI.UWP.Helpers;
using Drizzle.UI.UWP.Services;
using Drizzle.UI.UWP.ViewModels;
using Drizzle.UI.UWP.Views;
using Drizzle.Weather;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Globalization;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Drizzle.UI.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance for the current application instance.
        /// </summary>
        public static IServiceProvider Services
        {
            get
            {
                IServiceProvider serviceProvider = ((App)Current)._serviceProvider;
                return serviceProvider ?? throw new InvalidOperationException("The service provider is not initialized");
            }
        }
        public static bool IsDesktop => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop";
        public static bool IsTenFoot => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox" || _isTenFootPc;

        private readonly ILogger logger;
        private readonly IServiceProvider _serviceProvider;
        private static readonly bool _isTenFootPc = false;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            _serviceProvider = ConfigureServices();

            
            logger = Services.GetRequiredService<ILogger<App>>();
            var userSettings = Services.GetRequiredService<IUserSettings>();
            SetupUnhandledExceptionLogging();
            LogHardwareInformation();

            if (IsTenFoot)
            {
                // Ref: https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/how-to-disable-mouse-mode
                //this.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;

                // Ref: https://docs.microsoft.com/en-us/windows/uwp/design/input/gamepad-and-remote-interactions#reveal-focus
                this.FocusVisualKind = FocusVisualKind.Reveal;
            }

            if (SystemInfoUtil.Instance.IsFirstRun)
            {
                // Update before viewModel initialization
                var region = new GeographicRegion();
                switch (region.CodeTwoLetter)
                {
                    case "US":
                        userSettings.SetAndSerialize(UserSettingsConstants.WeatherUnit, WeatherUnits.imperial);
                        break;
                    case "GB":
                        userSettings.SetAndSerialize(UserSettingsConstants.WeatherUnit, WeatherUnits.hybrid);
                        break;
                }
            }
            else if (SystemInfoUtil.Instance.IsAppUpdated)
            {
                logger.LogInformation("App updated, performing maintenance..");
                // Clear cache incase any changes made to the impl
                Services.GetRequiredService<ICacheService>().Clear();
            }

            // For this application dark/light theme does not make sense
            // Interface design will be done assuming current theme is dark
            this.RequestedTheme = ApplicationTheme.Dark;
            //userSettings.SettingSet += (s, e) =>
            //{
            //    if (e == UserSettingsConstants.Theme)
            //        SetAppRequestedTheme();
            //};

            this.Suspending += OnSuspending;
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
                    Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "Cache"), 
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
                .AddTransient<AboutViewModel>()
                .AddTransient<SettingsViewModel>()
                .AddTransient<ScreensaverViewModel>()
                .AddTransient<DepthEstimateViewModel>()
                .AddTransient<IWeatherViewModelFactory, WeatherViewModelFactory>()
                .AddTransient<IWeatherClientFactory, WeatherClientFactory>()
                .AddTransient<IDownloadUtil, HttpDownloadUtil>()
                // https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
                .AddHttpClient()
                // Remove HttpClientFactory logging
                .RemoveAll<IHttpMessageHandlerBuilderFilter>()
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

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
                Services.GetRequiredService<INavigator>().RootFrame = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                CoreApplication.EnablePrelaunch(true);

                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(ShellPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
            //SetAppRequestedTheme();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        /// <summary>
        /// Method for setting requested app theme based on user's local settings.
        /// </summary>
        private void SetAppRequestedTheme()
        {
            // Note: this method must run after AppFrame has been assigned.
            var appFrame = Services.GetRequiredService<INavigator>().RootFrame as Frame;
            var theme = Services.GetRequiredService<IUserSettings>().GetAndDeserialize<AppTheme>(UserSettingsConstants.Theme);
            if (appFrame is not null)
            {
                appFrame.RequestedTheme = theme.ToTheme();
            }
        }

        private void SetupUnhandledExceptionLogging()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject);

            TaskScheduler.UnobservedTaskException += (s, e) =>
                LogUnhandledException(e.Exception);

            this.UnhandledException += (s, e) =>
                LogUnhandledException(e.Exception);

            Windows.ApplicationModel.Core.CoreApplication.UnhandledErrorDetected += (s, e) =>
                LogUnhandledException(e.UnhandledError);
        }

        private void LogHardwareInformation()
        {
            logger.LogInformation($"{SystemInfoUtil.Instance.ApplicationName} " +
                $"v{SystemInfoUtil.Instance.ApplicationVersion.Major}.{SystemInfoUtil.Instance.ApplicationVersion.Minor}" +
                $".{SystemInfoUtil.Instance.ApplicationVersion.Build}.{SystemInfoUtil.Instance.ApplicationVersion.Revision}");
            //logger.LogInformation($"OS: {SystemInformation.Instance.OperatingSystem} {SystemInformation.Instance.OperatingSystemVersion}, " +
            //    $"{SystemInformation.Instance.Culture}");
        }

        private void LogUnhandledException<T>(T exception) => logger.LogError(exception?.ToString());
    }
}
