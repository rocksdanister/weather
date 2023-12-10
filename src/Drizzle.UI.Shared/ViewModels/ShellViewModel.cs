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
using Drizzle.Weather;
using Drizzle.Weather.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Drizzle.UI.UWP.ViewModels
{
    public partial class ShellViewModel : ObservableObject
    {
        private readonly IUserSettings userSettings;
        private readonly IAssetReader assetReader;
        private readonly INavigator navigator;
        private readonly IDialogService dialogService;
        private readonly IWeatherClient weatherClient;
        private readonly ISoundService soundService;
        private readonly ICacheService cacheService;
        private readonly IWeatherViewModelFactory weatherViewModelFactory;
        private readonly ILogger logger;

        private readonly SemaphoreSlim weatherUpdatingLock = new(1, 1);
        private readonly int maxPinnedLocations;

        public ShellViewModel(IUserSettings userSettings,
            IAssetReader assetReader,
            INavigator navigator,
            ICacheService cacheService,
            IWeatherClient weatherClient,
            ISoundService soundService,
            IDialogService dialogService,
            IWeatherViewModelFactory weatherViewModelFactory,
            ILogger<ShellViewModel> logger)
        {
            this.userSettings = userSettings;
            this.assetReader = assetReader;
            this.navigator = navigator;
            this.dialogService = dialogService;
            this.weatherClient = weatherClient;
            this.soundService = soundService;
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

            WeathersSorted = new AdvancedCollectionView(Weathers, true);
            WeathersSorted.SortDescriptions.Add(new SortDescription(nameof(WeatherViewModel.SortOrder), SortDirection.Ascending));

            userSettings.SettingSet += async(s, e) => {
                switch (e)
                {
                    case UserSettingsConstants.Performance:
                        UpdateQualitySettings();
                        break;
                    case UserSettingsConstants.WeatherUnit:
                        await UpdateWeather();
                        break;
                    case UserSettingsConstants.BackgroundBrightness:
                        UpdateBrightness();
                        break;
                    case UserSettingsConstants.ReducedMotion:
                        UpdateMotionSettings();
                        break;
                }
            };

            navigator.ContentPageChanged += (s, e) => {
                IsMainPage = e == ContentPageType.Main;
                IsShowAddLocation = IsMainPage && SelectedLocation is null;
            };

            IsFirstRun = SystemInformation.Instance.IsFirstRun;
            IsAppUpdated = SystemInformation.Instance.IsAppUpdated;
            // For best user experience when volume is 0, pause audio.
            soundService.AutoPause = true;
            // Cache the weather data to reduce API calls when app is re-opened.
            weatherClient.UseCache = userSettings.Get<bool>(UserSettingsConstants.CacheWeather);
            maxPinnedLocations = userSettings.Get<int>(UserSettingsConstants.MaxPinnedLocations);
            IsReducedMotion = userSettings.Get<bool>(UserSettingsConstants.ReducedMotion);
            SoundVolume = userSettings.Get<int>(UserSettingsConstants.SoundVolume);

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
        private AdvancedCollectionView weathersSorted;

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
                if (SetProperty(ref _selectedLocation, value))
                {
                    SelectedWeather = value?.Today;
                    IsShowAddLocation = value is null;
                    if (value is not null && userSettings.GetAndDeserialize<LocationModel>(UserSettingsConstants.SelectedLocation) != value.Location)
                        userSettings.SetAndSerialize(UserSettingsConstants.SelectedLocation, value.Location);
                }
            }
        }


        private bool _isShowAddLocation;
        public bool IsShowAddLocation
        {
            get => _isShowAddLocation;
            set
            {
                if (SetProperty(ref _isShowAddLocation, value) && value)
                {
                    // Default animation
                    SetWeatherAnimation(WmoWeatherCode.ClearSky);
                }
            }
        }

        ///// <summary>
        ///// Is the user selected location currently daytime, if none selected uses system time.
        ///// </summary>
        public bool IsSelectedLocationDaytime
        {
            get => SelectedWeather?.IsDaytime ?? WeatherUtil.IsDaytime();
        }

        // Currently selected day
        private WeatherModel _selectedWeather;
        public WeatherModel SelectedWeather
        {
            get => _selectedWeather;
            set
            {
                if (SetProperty(ref _selectedWeather, value) && value is not null)
                {
                    SetWeatherAnimation((WmoWeatherCode)value.WeatherCode);
                }
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
        private bool isReducedMotion = false;

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

        [ObservableProperty]
        private bool isAppUpdated = false;

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

        private bool _isUpdatingWeather = false;
        public bool IsUpdatingWeather
        {
            get => _isUpdatingWeather;
            set
            {
                soundService.IsMuted = value;
                SetProperty(ref _isUpdatingWeather, value);
            }
        }

        [ObservableProperty]
        private bool isWorking = false;

        [ObservableProperty]
        private string errorMessage;
        
        private int _soundVolume;
        public int SoundVolume
        {
            get => _soundVolume;
            set
            {
                if (userSettings.Get<int>(UserSettingsConstants.SoundVolume) != value)
                    userSettings.Set(UserSettingsConstants.SoundVolume, value);

                soundService.Volume = value;
                SetProperty(ref _soundVolume, value);
            }
        }

        private bool CanRefreshCommand { get; set; } = true;

        [RelayCommand(CanExecute = nameof(CanRefreshCommand))]
        private async Task RefreshWeather()
        {
            try
            {
                CanRefreshCommand = false;
                RefreshWeatherCommand.NotifyCanExecuteChanged();

                cacheService.Clear();
                await UpdateWeather();
            }
            finally
            {
                CanRefreshCommand = true;
                RefreshWeatherCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand]
        private void DeleteLocation(WeatherViewModel obj)
        {
            if (obj is null)
                return;

            if (SelectedLocation == obj)
            {
                // If selected item is to be deleted, change selection first
                SelectedLocation = Weathers.Where(x => x != obj).OrderBy(x => x.SortOrder).FirstOrDefault();
            }
            Weathers.Remove(obj);
            using (WeathersSorted.DeferRefresh())
            {
                // Update SortOrder maximum values
                for (int i = 0; i < Weathers.Count; i++)
                {
                    Weathers[i].SortOrder = Weathers[i].SortOrder > obj.SortOrder ?
                        Weathers[i].SortOrder - 1 : Weathers[i].SortOrder;
                }
            }
            // Update saved pinned locations
            StoreLocationsSorted();
        }

        [RelayCommand]
        private void OpenScreensaver() => navigator.ToScreensaver();

        [RelayCommand]
        private async Task OpenHelp() => await dialogService.ShowHelpDialogAsync();

        [RelayCommand]
        private async Task OpenAbout() => await dialogService.ShowAboutDialogAsync();

        [RelayCommand]
        private async Task OpenSettings() => await dialogService.ShowSettingsDialogAsync();

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
                var weatherVm = weatherViewModelFactory.CreateWeatherViewModel(weather, airQuality, Weathers.Count, units);
                Weathers.Add(weatherVm);

                // Selects location and weather animation
                SelectedLocation = weatherVm;
                // Force weather animation background change
                //SetWeatherAnimation((WmoWeatherCode)SelectedLocation.Today.WeatherCode, true);
                // For selection only, animation change is skipped
                //SelectedWeather = SelectedLocation.Daily[0];

                StoreLocationsSorted();
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
                IsUpdatingWeather = true;
                var locations = userSettings.GetAndDeserialize<LocationModel[]>(UserSettingsConstants.PinnedLocations).Take(maxPinnedLocations).ToList();
                var selection = userSettings.GetAndDeserialize<LocationModel>(UserSettingsConstants.SelectedLocation);
                var units = userSettings.GetAndDeserialize<WeatherUnits>(UserSettingsConstants.WeatherUnit);

                // Set default animation, setting it here instead of constructor to avoid ComputeSharp/UWP Windowsize issue.
                IsShowAddLocation = true;

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
                    Weathers.Add(weatherViewModelFactory.CreateWeatherViewModel(weather, airQuality, 0, units));
                }

                var index = locations.FindIndex(x => x.Latitude == selection.Latitude && x.Longitude == selection.Longitude);
                if (index == -1)
                {
                    // Selected location not found, this can happen if max pinned location is reduced from the previous.
                    // We will remove last location to keep max pinned count same since when loading locations we only take max pinned.
                    Weathers.RemoveAt(Weathers.Count - 1);
                    // Selection will be first fetched item
                    index = 0;
                }
                // Reorder since we fetched the selection first, DeferRefresh is causing issue
                Weathers[0].SortOrder = index;
                for (int i = 1; i < Weathers.Count; i++)
                {
                    Weathers[i].SortOrder = i > index ? i : i - 1;
                }
                // Update order
                StoreLocationsSorted();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
            finally
            {
                IsUpdatingWeather = false;
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

                IsUpdatingWeather = true;
                var units = userSettings.GetAndDeserialize<WeatherUnits>(UserSettingsConstants.WeatherUnit);
                var weatherCopy = Weathers.OrderBy(x => x.SortOrder).ToList();
                var selectionCopy = SelectedLocation;
                Weathers.Clear();

                for (int i = 0; i < weatherCopy.Count; i++)
                {
                    // Uses cache if configured and unit convertion takes place in the viewmodel
                    (var weather, var airQuality) = await FetchWeather(weatherCopy[i].Location.Name, weatherCopy[i].Location.Latitude, weatherCopy[i].Location.Longitude);
                    Weathers.Add(weatherViewModelFactory.CreateWeatherViewModel(weather, airQuality, i, units));
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
                IsUpdatingWeather = false;
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
                // Some providers don't have Reverse Geocoding api
                weather.Name ??= name;
                airQuality.Name ??= name;
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
            var obj = code.GetWeather();
            // Reduce overall animation speed
            var speedFactor = IsReducedMotion ? 0.35f : 1.0f;
            // Fallback to available asset if day/night not available
            var randomBackground = assetReader.GetRandomBackground(obj.Type, IsSelectedLocationDaytime) ?? assetReader.GetRandomBackground(obj.Type);
            switch (obj.Type)
            {
                case ShaderTypes.clouds:
                    {
                        var property = obj as CloudsModel;
                        CloudsProperty.Scale = property.Scale;
                        CloudsProperty.Iterations = property.Iterations;
                        CloudsProperty.Speed = property.Speed * speedFactor;
                        CloudsProperty.IsDaytime = IsSelectedLocationDaytime;
                        CloudsProperty.IsDayNightShift = property.IsDayNightShift;
                    }
                    break;
                case ShaderTypes.rain:
                    {
                        var property = obj as RainModel;
                        RainProperty.Zoom = property.Zoom;
                        RainProperty.Normal = property.Normal;
                        RainProperty.Speed = property.Speed * speedFactor;
                        RainProperty.Intensity = property.Intensity;
                        RainProperty.PostProcessing =property.PostProcessing;
                        RainProperty.IsLightning = property.IsLightning;
                        RainProperty.IsPanning = property.IsPanning;
                        RainProperty.IsFreezing = property.IsFreezing;
                        // If force update, loaded background is not the required day/night type or texture missing.
                        if (updateImage || RainProperty.IsDaytime != IsSelectedLocationDaytime || RainProperty.ImagePath is null)
                        {
                            RainProperty.ImagePath = randomBackground.FilePath;
                        }
                        RainProperty.IsDaytime = IsSelectedLocationDaytime;
                    }
                    break;
                case ShaderTypes.snow:
                    {
                        var property = obj as SnowModel;
                        SnowProperty.Layers = property.Layers;
                        SnowProperty.Depth = property.Depth;
                        SnowProperty.Speed = property.Speed * speedFactor;
                        SnowProperty.Width = property.Width;
                        SnowProperty.IsBlur = property.IsBlur;
                        SnowProperty.IsLightning = property.IsLightning;
                        SnowProperty.PostProcessing = property.PostProcessing;

                        if (updateImage || SnowProperty.IsDaytime != IsSelectedLocationDaytime || SnowProperty.ImagePath is null)
                        {
                            SnowProperty.ImagePath = randomBackground.FilePath;
                        }
                        SnowProperty.IsDaytime = IsSelectedLocationDaytime;
                    }
                    break;
                case ShaderTypes.depth:
                    {
                        var property = obj as DepthModel;
                        DepthProperty.IntensityX = property.IntensityX;
                        DepthProperty.IntensityY = property.IntensityY;
                        DepthProperty.IsBlur = property.IsBlur;

                        if (updateImage || DepthProperty.IsDaytime != IsSelectedLocationDaytime || (DepthProperty.ImagePath is null || DepthProperty.DepthPath is null))
                        {
                            DepthProperty.ImagePath = randomBackground.FilePath;
                            DepthProperty.DepthPath = randomBackground.DepthPath;
                        }
                        DepthProperty.IsDaytime = IsSelectedLocationDaytime;
                    }
                    break;
                case ShaderTypes.fog:
                    { 
                        var property = obj as WindModel;
                        FogProperty.Color1 = property.Color1;
                        FogProperty.Color2 = property.Color2;
                        FogProperty.Speed = property.Speed * speedFactor;
                        FogProperty.Amplitude = property.Amplitude;
                        FogProperty.ParallaxIntensityX = property.ParallaxIntensityX;
                        FogProperty.ParallaxIntensityY = property.ParallaxIntensityY;

                        if (updateImage || FogProperty.IsDaytime != IsSelectedLocationDaytime || (FogProperty.ImagePath is null || FogProperty.DepthPath is null))
                        {
                            FogProperty.ImagePath = randomBackground.FilePath;
                            FogProperty.DepthPath = randomBackground.DepthPath;
                        }
                        FogProperty.IsDaytime = IsSelectedLocationDaytime;
                    }
                    break;
            }
            SetShader(obj.Type);
            SetWeatherSound(code);
            SelectedWeatherAnimation = code;
            FallbackBackground = IsFallbackBackground ?
                // Can be null if shader has no texture input
                randomBackground?.FilePath ??
                // Pick depth asset as replacement for missing texture
                assetReader.GetRandomBackground(ShaderTypes.depth, IsSelectedLocationDaytime)?.FilePath ??
                // Fallback to available asset if day/night depth asset not available
                assetReader.GetRandomBackground(ShaderTypes.depth)?.FilePath :
                // Fallback background disabled
                null;
        }

        private void SetWeatherSound(WmoWeatherCode code)
        {
            soundService.SetSource(code, IsSelectedLocationDaytime);
            soundService.Play();
        }

        /// <summary>
        /// Update all weather backgrounds to random image from shuffle
        /// </summary>
        public void RandomizeWeatherBackgrounds()
        {
            // Fallback to available asset if day/night not available
            var fogAsset = assetReader.GetRandomBackground(ShaderTypes.fog, FogProperty.IsDaytime) ?? assetReader.GetRandomBackground(ShaderTypes.fog);
            var depthAsset = assetReader.GetRandomBackground(ShaderTypes.depth, DepthProperty.IsDaytime) ?? assetReader.GetRandomBackground(ShaderTypes.depth);
            var rainAsset = assetReader.GetRandomBackground(ShaderTypes.rain, RainProperty.IsDaytime) ?? assetReader.GetRandomBackground(ShaderTypes.rain);
            var snowAsset = assetReader.GetRandomBackground(ShaderTypes.snow, SnowProperty.IsDaytime) ?? assetReader.GetRandomBackground(ShaderTypes.snow);

            FogProperty.ImagePath = fogAsset.FilePath;
            FogProperty.DepthPath = fogAsset.DepthPath;
            DepthProperty.ImagePath = depthAsset.FilePath;
            DepthProperty.DepthPath = depthAsset.DepthPath;

            RainProperty.ImagePath = rainAsset.FilePath;
            SnowProperty.ImagePath = snowAsset.FilePath;
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

        private void UpdateMotionSettings()
        {
            IsReducedMotion = userSettings.Get<bool>(UserSettingsConstants.ReducedMotion);
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

        private void StoreLocationsSorted()
        {
            var sortedLocations = Weathers.OrderBy(x => x.SortOrder).Select(x => x.Location).ToArray();
            userSettings.SetAndSerialize(UserSettingsConstants.PinnedLocations, sortedLocations);
        }
    }
}
