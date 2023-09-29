using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputeSharp;
using Drizzle.Common;
using Drizzle.Common.Constants;
using Drizzle.Common.Services;
using Drizzle.Models;
using Drizzle.Models.Weather;
using Drizzle.UI.Shared.Shaders.Helpers;
using Drizzle.UI.Shared.Shaders.Models;
using Drizzle.UI.Shared.Shaders.Runners;
using Drizzle.UI.UWP.Factories;
using Drizzle.UI.UWP.Views;
using Drizzle.Weather;
using Drizzle.Weather.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Drizzle.UI.UWP.ViewModels
{
    public partial class ShellViewModel : ObservableObject
    {
        private readonly IUserSettings userSettings;
        private readonly INavigator navigator;
        private readonly IDialogService dialogService;
        private readonly IWeatherClient weatherClient;
        private readonly ICacheService cacheService;
        private readonly IWeatherViewModelFactory weatherViewModelFactory;
        private readonly ILogger logger;

        private readonly SemaphoreSlim weatherUpdatingLock = new(1, 1);
        private readonly Random rnd = new();
        private readonly int maxPinnedLocations;

        public ShellViewModel(IUserSettings userSettings,
            INavigator navigator,
            ICacheService cacheService,
            IWeatherClient weatherClient,
            IDialogService dialogService,
            IWeatherViewModelFactory weatherViewModelFactory,
            ILogger<ShellViewModel> logger)
        {
            this.userSettings = userSettings;
            this.navigator = navigator;
            this.dialogService = dialogService;
            this.weatherClient = weatherClient;
            this.cacheService = cacheService;
            this.weatherViewModelFactory = weatherViewModelFactory;
            this.logger = logger;
       
            shaderRunnerViewModels = new ShaderRunnerViewModel[]{
                new ShaderRunnerViewModel(new SnowRunner(() => SnowProperty), ShaderTypes.snow, scaleFactor: 0.75f, maxScaleFactor: 1f),
                new ShaderRunnerViewModel(new CloudsRunner(() => CloudsProperty), ShaderTypes.clouds, scaleFactor: 0.2f, maxScaleFactor: 0.4f),
                new ShaderRunnerViewModel(new RainRunner(() => RainProperty), ShaderTypes.rain, scaleFactor: 0.75f, maxScaleFactor: 1f),
                new ShaderRunnerViewModel(new DepthRunner(() => DepthProperty), ShaderTypes.depth, scaleFactor: 1f, maxScaleFactor: 1f),
                new ShaderRunnerViewModel(new WindRunner(() => FogProperty), ShaderTypes.fog, scaleFactor: 0.75f, maxScaleFactor: 1f),
            };

            userSettings.SettingSet += async(s, e) => {
                if (e == UserSettingsConstants.Performance)
                    UpdateQualitySettings();
                else if (e == UserSettingsConstants.WeatherUnit)
                    await UpdateWeather();
                else if (e == UserSettingsConstants.BackgroundBrightness)
                    UpdateBrightness();
            };

            navigator.ContentPageChanged += (s, e) => {
                IsMainPage = e == ContentPageType.Main;
                IsShowAddLocation = IsMainPage && SelectedLocation is null;
            };

            IsFirstRun = SystemInformation.Instance.IsFirstRun;
            weatherClient.UseCache = userSettings.Get<bool>(UserSettingsConstants.CacheWeather);
            maxPinnedLocations = userSettings.Get<int>(UserSettingsConstants.MaxPinnedLocations);

            var gpu = GraphicsDevice.GetDefault();
            IsHardwareAccelerated = gpu.IsHardwareAccelerated;
            logger.LogInformation($"GPU: {gpu.Name}, Hardware acceleration: {gpu.IsHardwareAccelerated}");

            var quality = userSettings.GetAndDeserialize<AppPerformance>(UserSettingsConstants.Performance);
            IsFallbackBackground = quality == AppPerformance.potato || !IsHardwareAccelerated;
            // Alert user only on first run
            IsHardwareAccelerationMissingNotify = !IsHardwareAccelerated && IsFirstRun;
        }

        private readonly IReadOnlyList<ShaderRunnerViewModel> shaderRunnerViewModels;

        [ObservableProperty]
        private ObservableCollection<Location> searchSuggestions = new();

        [ObservableProperty]
        private ObservableCollection<WeatherViewModel> weathers = new();

        [ObservableProperty]
        private WmoWeatherCode selectedWeatherAnimation;

        private WeatherViewModel _selectedLocation;
        public WeatherViewModel SelectedLocation
        {
            get => _selectedLocation;
            set
            {
                if (value is not null)
                {
                    IsShowAddLocation = false;
                    if (userSettings.GetAndDeserialize<LocationModel>(UserSettingsConstants.SelectedLocation) != value.Location)
                        userSettings.SetAndSerialize(UserSettingsConstants.SelectedLocation, value.Location);
                }
                else
                {
                    IsShowAddLocation = true;
                    // Default animation
                    SetWeatherAnimation(WmoWeatherCode.ClearSky);
                }
                SelectedWeather = value?.Today;
                SetProperty(ref _selectedLocation, value);
            }
        }

        // Currently selected day
        private WeatherModel _selectedWeather;
        public WeatherModel SelectedWeather
        {
            get => _selectedWeather;
            set
            {
                if (value is not null)
                {
                    SetWeatherAnimation((WmoWeatherCode)value.WeatherCode);
                }
                SetProperty(ref _selectedWeather, value);
            }
        }

        [ObservableProperty]
        private string fallbackBackground;

        /// <summary>
        /// Disable shader animation and use fallback image instead
        /// </summary>
        [ObservableProperty]
        private bool isFallbackBackground = false;

        [ObservableProperty]
        private CloudsModel cloudsProperty = new();

        [ObservableProperty]
        private DepthModel depthProperty = new();

        [ObservableProperty]
        private RainModel rainProperty = new();

        [ObservableProperty]
        private SnowModel snowProperty = new();

        [ObservableProperty]
        private WindModel fogProperty = new();

        [ObservableProperty]
        private ShaderRunnerViewModel selectedShader1;

        [ObservableProperty]
        private ShaderRunnerViewModel selectedShader2;

        [ObservableProperty]
        private float resolutionScaleShader1 = 1f;

        [ObservableProperty]
        private float resolutionScaleShader2 = 1f;

        [ObservableProperty]
        private bool isPausedShader1 = false;

        [ObservableProperty]
        private bool isPausedShader2 = true;

        [ObservableProperty]
        private bool isDynamicResolution = true;

        [ObservableProperty]
        private bool isHardwareAccelerated = true;

        [ObservableProperty]
        private bool isHardwareAccelerationMissingNotify = false;

        [ObservableProperty]
        private bool isFirstRun = false;

        [ObservableProperty]
        private bool isMainPage = true;

        /// <summary>
        /// Animate shader on mouse drag
        /// </summary>
        [ObservableProperty]
        private bool isMouseDrag = false;

        [ObservableProperty]
        private bool isWeatherInputPaneOpen = false;

        [ObservableProperty]
        private bool isFetchingWeather = false;

        [ObservableProperty]
        private bool isFetchingLocation = false;

        [ObservableProperty]
        private bool isWorking = false;

        [ObservableProperty]
        private bool isShowAddLocation = false;

        [ObservableProperty]
        private string errorMessage;

        private bool canRefreshCommand = true;
        private RelayCommand _refreshCommand;
        public RelayCommand RefreshCommand => _refreshCommand ??= new RelayCommand(async() => {
            try
            {
                canRefreshCommand = false;
                RefreshCommand.NotifyCanExecuteChanged();

                cacheService.Clear();
                await UpdateWeather();
            }
            finally
            {
                canRefreshCommand = true;
                RefreshCommand.NotifyCanExecuteChanged();
            }
        }, () => canRefreshCommand);

        private RelayCommand<WeatherViewModel> _deleteLocationCommand;
        public RelayCommand<WeatherViewModel> DeleteLocationCommand => _deleteLocationCommand ??= new RelayCommand<WeatherViewModel>((obj) => {
            if (obj is not null)
            {
                Weathers.Remove(obj);
                // If selecteditem is deleted
                SelectedLocation ??= Weathers.Any() ? Weathers[0] : null;
                userSettings.SetAndSerialize(UserSettingsConstants.PinnedLocations, Weathers.Select(x => x.Location).ToArray());
            }
        });

        private RelayCommand _screensaverCommand;
        public RelayCommand ScreensaverCommand => _screensaverCommand ??= new RelayCommand(() => navigator.ToScreensaver());

        private RelayCommand _helpCommand;
        public RelayCommand HelpCommand => _helpCommand ??= new RelayCommand(async () => await dialogService.ShowHelpDialogAsync());

        private RelayCommand _aboutCommand;
        public RelayCommand AboutCommand => _aboutCommand ??= new RelayCommand(async () => await dialogService.ShowAboutDialogAsync());

        private RelayCommand _settingsCommand;
        public RelayCommand SettingsCommand => _settingsCommand ??= new RelayCommand(async () => await dialogService.ShowSettingsDialogAsync());

        /// <summary>
        /// Fetch and set weather animations.
        /// </summary>
        public async Task SetWeather(string name, float latitude, float longitude)
        {
            try
            {
                var search = Weathers.FirstOrDefault(x => x.Location.Latitude == latitude && x.Location.Longitude == longitude);
                if (search is not null)
                {
                    SelectedLocation = search;
                    return;
                }

                (var weather, var airQuality) = await FetchWeather(name, latitude, longitude);
                // Only store required data
                if (Weathers.Count >= maxPinnedLocations)
                    Weathers.RemoveAt(0);

                // Update view
                var units = userSettings.GetAndDeserialize<WeatherUnits>(UserSettingsConstants.WeatherUnit);
                var weatherVm = weatherViewModelFactory.CreateWeatherViewModel(weather, airQuality, units);
                Weathers.Add(weatherVm);

                // Selects location and weather animation
                SelectedLocation = weatherVm;
                // Force weather animation background change
                SetWeatherAnimation((WmoWeatherCode)SelectedLocation.Daily[0].WeatherCode, true);
                // For selection only, animation change is skipped
                //SelectedWeather = SelectedLocation.Daily[0];

                var locations = Weathers.Select(x => x.Location);
                userSettings.SetAndSerialize(UserSettingsConstants.PinnedLocations, locations.ToArray());
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                ErrorMessage = ex.ToString();
            }
        }

        /// <summary>
        /// Restore/fetch saved weather information (based on cache configuration.)
        /// </summary>
        public async Task RestoreWeather()
        {
            try
            {
                var locations = userSettings.GetAndDeserialize<LocationModel[]>(UserSettingsConstants.PinnedLocations).Take(maxPinnedLocations).ToList();
                var selection = userSettings.GetAndDeserialize<LocationModel>(UserSettingsConstants.SelectedLocation);
                var units = userSettings.GetAndDeserialize<WeatherUnits>(UserSettingsConstants.WeatherUnit);

                // Set default animation, setting it here instead of constructor to avoid ComputeSharp/UWP Windowsize issue.
                SelectedLocation = null;

                if (!locations.Any())
                {
                    return;
                }

                // Fetch/load the selection first for better UX
                selection ??= locations[0];
                await SetWeather(selection.Name, selection.Latitude, selection.Longitude);

                // Fetch/Load the rest in the background
                var remainingLocations = locations.Where(x => x.Latitude != selection.Latitude && x.Longitude != selection.Longitude);
                foreach (var location in remainingLocations)
                {
                    (var weather, var airQuality) = await FetchWeather(location.Name, location.Latitude, location.Longitude);
                    Weathers.Add(weatherViewModelFactory.CreateWeatherViewModel(weather, airQuality, units));
                }

                // Reorder since we fetched the selection first
                var index = locations.FindIndex(x => x.Latitude == selection.Latitude && x.Longitude == selection.Longitude);
                if (index > 0)
                {
                    var tmp = Weathers[0];
                    Weathers.RemoveAt(0);
                    Weathers.Insert(index, tmp);
                    SelectedLocation = tmp;
                }

                // Update order
                userSettings.SetAndSerialize(UserSettingsConstants.PinnedLocations, Weathers.Select(x => x.Location).ToArray());
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }

        private async Task UpdateWeather()
        {
            try
            {
                // In case cache is outdated, new fetch request takes time.. so to avoid user spamming during.
                await weatherUpdatingLock.WaitAsync();

                if (!Weathers.Any())
                    return;

                var units = userSettings.GetAndDeserialize<WeatherUnits>(UserSettingsConstants.WeatherUnit);
                var weatherCopy = Weathers.ToList();
                var selectionCopy = SelectedLocation;
                Weathers.Clear();

                for (int i = 0; i < weatherCopy.Count; i++)
                {
                    // Uses cache if configured and convertion takes place in the viewmodel
                    (var weather, var airQuality) = await FetchWeather(weatherCopy[i].Location.Name, weatherCopy[i].Location.Latitude, weatherCopy[i].Location.Longitude);
                    Weathers.Add(weatherViewModelFactory.CreateWeatherViewModel(weather, airQuality, units));
                }
                SelectedLocation = Weathers.FirstOrDefault(x => x.Location.Latitude == selectionCopy.Location.Latitude && x.Location.Longitude == selectionCopy.Location.Longitude);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
            finally
            {
                weatherUpdatingLock.Release();
            }
        }

        private async Task<(ForecastWeather, ForecastAirQuality)> FetchWeather(string name, float latitude, float longitude)
        {
            try
            {
                IsWorking = true;
                IsFetchingWeather = true;
                // Uses disk cache if configured
                var weather = await weatherClient.QueryForecastAsync(latitude, longitude);
                var airQuality = await weatherClient.QueryAirQualityAsync(latitude, longitude);
                // Reverse Geo-location api not available currently
                weather.Name = name;
                airQuality.Name = name;
                return (weather, airQuality);
            }
            finally
            {
                IsWorking = false;
                IsFetchingWeather = false;
            }
        }

        public async Task FetchLocations(string search)
        {
            if (string.IsNullOrEmpty(search) || search.Length < 2)
                return;

            try
            {
                IsWorking = true;
                IsFetchingLocation = true;
                var locations = await weatherClient.GetLocationDataAsync(search);
                SearchSuggestions.Clear();
                foreach (var item in locations)
                {
                    SearchSuggestions.Add(item);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                ErrorMessage = ex.ToString();
            }
            finally
            {
                IsWorking = false;
                IsFetchingLocation = false;
            }
        }

        /// <summary>
        /// Update weather animation (irrespective of current weather.)
        /// </summary>
        /// <param name="code">Wmo weather code.</param>
        /// <param name="updateImage">Force background change, use existing background otherwise.</param>
        public void SetWeatherAnimation(WmoWeatherCode code, bool updateImage = false)
        {
            string randomBackground = null;
            var obj = code.GetWeather();
            switch (obj.Type)
            {
                case ShaderTypes.clouds:
                    {
                        var property = obj as CloudsModel;
                        CloudsProperty.Scale = property.Scale;
                        CloudsProperty.Iterations = property.Iterations;
                        CloudsProperty.Speed = property.Speed;
                        CloudsProperty.IsDayNightShift = property.IsDayNightShift;
                    }
                    break;
                case ShaderTypes.rain:
                    {
                        var property = obj as RainModel;
                        RainProperty.Zoom = property.Zoom;
                        RainProperty.Normal = property.Normal;
                        RainProperty.Speed = property.Speed;
                        RainProperty.Intensity = property.Intensity;
                        RainProperty.PostProcessing =property.PostProcessing;
                        RainProperty.IsLightning = property.IsLightning;
                        RainProperty.IsPanning = property.IsPanning;
                        RainProperty.IsFreezing = property.IsFreezing;

                        randomBackground = AssetImageConstants.RainAssets[rnd.Next(AssetImageConstants.RainAssets.Count)];
                        if (updateImage || RainProperty.ImagePath is null)
                            RainProperty.ImagePath = randomBackground;
                    }
                    break;
                case ShaderTypes.snow:
                    {
                        var property = obj as SnowModel;
                        SnowProperty.Layers = property.Layers;
                        SnowProperty.Depth = property.Depth;
                        SnowProperty.Speed = property.Speed;
                        SnowProperty.Width = property.Width;
                        SnowProperty.IsBlur = property.IsBlur;
                        SnowProperty.IsLightning = property.IsLightning;
                        SnowProperty.PostProcessing = property.PostProcessing;

                        randomBackground = AssetImageConstants.SnowAssets[rnd.Next(AssetImageConstants.SnowAssets.Count)];
                        if (updateImage || SnowProperty.ImagePath is null)
                            SnowProperty.ImagePath = randomBackground;
                    }
                    break;
                case ShaderTypes.depth:
                    {
                        var property = obj as DepthModel;
                        DepthProperty.IntensityX = property.IntensityX;
                        DepthProperty.IntensityY = property.IntensityY;
                        DepthProperty.IsBlur = property.IsBlur;

                        var index = rnd.Next(AssetImageConstants.DepthAssets.Count);
                        randomBackground = Path.Combine(AssetImageConstants.DepthAssets[index], "image.jpg");
                        if (updateImage || (DepthProperty.ImagePath is null || DepthProperty.DepthPath is null))
                        {
                            DepthProperty.ImagePath = randomBackground;
                            DepthProperty.DepthPath = Path.Combine(AssetImageConstants.DepthAssets[index], "depth.jpg");
                        }
                    }
                    break;
                case ShaderTypes.fog:
                    { 
                        var property = obj as WindModel;
                        FogProperty.Color1 = property.Color1;
                        FogProperty.Color2 = property.Color2;
                        FogProperty.Speed = property.Speed;
                        FogProperty.Amplitude = property.Amplitude;
                        FogProperty.ParallaxIntensityX = property.ParallaxIntensityX;
                        FogProperty.ParallaxIntensityY = property.ParallaxIntensityY;

                        var index = rnd.Next(AssetImageConstants.DepthAssets.Count);
                        randomBackground = Path.Combine(AssetImageConstants.DepthAssets[index], "image.jpg");
                        if (updateImage || (FogProperty.ImagePath is null || FogProperty.DepthPath is null))
                        {
                            FogProperty.ImagePath = randomBackground;
                            FogProperty.DepthPath = Path.Combine(AssetImageConstants.DepthAssets[index], "depth.jpg");
                        }
                    }
                    break;
            }
            SetShader(obj.Type);
            SelectedWeatherAnimation = code;
            FallbackBackground = IsFallbackBackground ? 
                randomBackground ?? Path.Combine(AssetImageConstants.DepthAssets[rnd.Next(AssetImageConstants.DepthAssets.Count)], "image.jpg") : null;
        }

        /// <summary>
        /// Update all weather backgrounds to random image from shuffle
        /// </summary>
        public void RandomWeatherAnimationBackgrounds()
        {
            var fogIndex = rnd.Next(AssetImageConstants.DepthAssets.Count);
            var depthIndex = rnd.Next(AssetImageConstants.DepthAssets.Count);
            FogProperty.ImagePath = Path.Combine(AssetImageConstants.DepthAssets[fogIndex], "image.jpg");
            FogProperty.DepthPath = Path.Combine(AssetImageConstants.DepthAssets[fogIndex], "depth.jpg");
            DepthProperty.ImagePath = Path.Combine(AssetImageConstants.DepthAssets[depthIndex], "image.jpg");
            DepthProperty.DepthPath = Path.Combine(AssetImageConstants.DepthAssets[depthIndex], "depth.jpg");

            RainProperty.ImagePath = AssetImageConstants.RainAssets[rnd.Next(AssetImageConstants.RainAssets.Count)];
            SnowProperty.ImagePath = AssetImageConstants.SnowAssets[rnd.Next(AssetImageConstants.SnowAssets.Count)];
        }

        /// <summary>
        /// Switches shader with transition effect by using two panels
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private void SetShader(ShaderTypes type)
        {
            // CPU emulation, skip for better UX
            if (!IsHardwareAccelerated)
                return;

            // Turn off all animations
            if (IsFallbackBackground)
            {
                IsPausedShader1 = true;
                IsPausedShader2 = true;

                // Turn off shader if running
                SelectedShader1 = SelectedShader2 = null;

                return;
            }

            // Skip if running already
            if (SelectedShader2?.ShaderType == type || SelectedShader1?.ShaderType == type)
                return;

            var selection = shaderRunnerViewModels.First(x => x.ShaderType == type);
            if (SelectedShader1 is null)
            {
                SelectedShader1 = selection;
                IsPausedShader1 = false;
                IsPausedShader2 = true;

                SelectedShader2 = null;
            }
            else
            {
                SelectedShader2 = selection;
                IsPausedShader1 = true;
                IsPausedShader2 = false;

                SelectedShader1 = null;
            }
            UpdateShaderScale();
            UpdateBrightness();

            // Reset mouse
            RainProperty.Mouse =
                CloudsProperty.Mouse =
                SnowProperty.Mouse =
                FogProperty.Mouse =
                DepthProperty.Mouse = Float4.Zero;
        }

        private void UpdateQualitySettings()
        {
            var quality = userSettings.GetAndDeserialize<AppPerformance>(UserSettingsConstants.Performance);
            IsFallbackBackground = quality == AppPerformance.potato || !IsHardwareAccelerated;
            SetWeatherAnimation(SelectedWeatherAnimation);
        }

        private void UpdateShaderScale()
        {
            switch (userSettings.GetAndDeserialize<AppPerformance>(UserSettingsConstants.Performance))
            {
                case AppPerformance.performance:
                    {
                        IsDynamicResolution = false;
                        if (SelectedShader1 is not null)
                            ResolutionScaleShader1 = SelectedShader1.ScaleFactor;
                        else if (SelectedShader2 is not null)
                            ResolutionScaleShader2 = SelectedShader2.ScaleFactor;
                    }
                    break;
                case AppPerformance.quality:
                    {
                        IsDynamicResolution = false;
                        if (SelectedShader1 is not null)
                            ResolutionScaleShader1 = SelectedShader1.MaxScaleFactor;
                        else if (SelectedShader2 is not null)
                            ResolutionScaleShader2 = SelectedShader2.MaxScaleFactor;
                    }
                    break;
                case AppPerformance.dynamic:
                    {
                        IsDynamicResolution = true;
                    }
                    break;
            }
        }

        private void UpdateBrightness()
        {
            var brightness = userSettings.Get<float>(UserSettingsConstants.BackgroundBrightness);
            RainProperty.Brightness =
                SnowProperty.Brightness =
                FogProperty.Brightness =
                DepthProperty.Brightness =
                CloudsProperty.Brightness = brightness;
        }
    }
}
