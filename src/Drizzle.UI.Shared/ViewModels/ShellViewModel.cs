using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drizzle.Common.Constants;
using Drizzle.Common.Services;
using Drizzle.Models;
using Drizzle.Models.Enums;
using Drizzle.Models.Shaders;
using Drizzle.Models.Weather;
using Drizzle.Weather;
using Drizzle.Weather.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using Drizzle.UI.Shared.Factories;
using Drizzle.UI.Shared.Extensions;
using Drizzle.Common.Helpers;

#if WINDOWS_UWP
using CommunityToolkit.WinUI.Collections;
#endif

namespace Drizzle.UI.Shared.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly IUserSettings userSettings;
    private readonly IAssetReader assetReader;
    private readonly INavigator navigator;
    private readonly IDialogService dialogService;
    private readonly ISoundService soundService;
    private readonly ICacheService cacheService;
    private readonly IWeatherClientFactory weatherClientFactory;
    private readonly IWeatherViewModelFactory weatherViewModelFactory;
    private readonly IGeolocationService geolocationService;
    private readonly ITimerService weatherRefreshTimer;
    private readonly ILogger logger;

    private readonly WmoWeatherCode defaultAnimation = WmoWeatherCode.MainlyClear;
    private readonly SemaphoreSlim weatherUpdatingLock = new(1, 1);
    private readonly int maxPinnedLocations;

    private IWeatherClient weatherClient;

    public ShellViewModel(IUserSettings userSettings,
        IAssetReader assetReader,
        INavigator navigator,
        ICacheService cacheService,
        IWeatherClientFactory weatherClientFactory,
        ISoundService soundService,
        IDialogService dialogService,
        IShaderViewModelFactory shaderViewModelFactory,
        IWeatherViewModelFactory weatherViewModelFactory,
        IGeolocationService geolocationService,
        ISystemInfoProvider systemInfo,
        IAppUpdaterService appUpdater,
        ITimerFactory timerFactory,
        ILogger<ShellViewModel> logger)
    {
        this.userSettings = userSettings;
        this.assetReader = assetReader;
        this.navigator = navigator;
        this.dialogService = dialogService;
        this.soundService = soundService;
        this.cacheService = cacheService;
        this.weatherClientFactory = weatherClientFactory;
        this.weatherViewModelFactory = weatherViewModelFactory;
        this.geolocationService = geolocationService;
        this.logger = logger;

        // User selected weather provider.
        this.weatherClient = weatherClientFactory.GetInstance(userSettings.GetAndDeserialize<WeatherProviders>(UserSettingsConstants.SelectedWeatherProvider));
        this.IsShowDetectLocation = weatherClient.IsReverseGeocodingSupported;

        // Store for easy databinding customisation (screensaver.)
        var cloudsVm = shaderViewModelFactory.Create(ShaderTypes.clouds);
        CloudsProperty = cloudsVm.Model as CloudsModel;
        var rainVm = shaderViewModelFactory.Create(ShaderTypes.rain);
        RainProperty = rainVm.Model as RainModel;
        var snowVm = shaderViewModelFactory.Create(ShaderTypes.snow);
        SnowProperty = snowVm.Model as SnowModel;
        var depthVm = shaderViewModelFactory.Create(ShaderTypes.depth);
        DepthProperty = depthVm.Model as DepthModel;
        var fogVm = shaderViewModelFactory.Create(ShaderTypes.fog);
        FogProperty = fogVm.Model as WindModel;

        ShaderViewModels = [
            cloudsVm,
            rainVm,
            snowVm,
            depthVm,
            fogVm
        ];

#if WINDOWS_UWP
        WeathersSorted = new AdvancedCollectionView(Weathers, true);
        WeathersSorted.SortDescriptions.Add(new SortDescription(nameof(WeatherViewModel.SortOrder), SortDirection.Ascending));
#endif

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
                case UserSettingsConstants.SelectedWeatherProvider:
                    await UpdateWeatherProvider();
                    break;
                case UserSettingsConstants.SelectedShaderRenderer:
                    UpdateShaderRenderer();
                    break;
                case UserSettingsConstants.TargetFrameRate:
                    TargetFrameRate = userSettings.Get<int>(UserSettingsConstants.TargetFrameRate);
                    break;
                case UserSettingsConstants.DiagnosticsVisible:
                    IsDiagnosticVisible = userSettings.Get<bool>(UserSettingsConstants.DiagnosticsVisible);
                    break;
            }
        };

        navigator.ContentPageChanged += (s, e) => {
            IsMainPage = e == ContentPageType.Main;
            IsShowAddLocation = IsMainPage && SelectedLocation is null;
        };

        // For best user experience when volume is 0, pause audio.
        soundService.AutoPause = true;
        // Cache the weather data to reduce API calls when app is re-opened.
        cacheService.UseCache = userSettings.Get<bool>(UserSettingsConstants.CacheWeather);
        maxPinnedLocations = userSettings.Get<int>(UserSettingsConstants.MaxPinnedLocations);
        IsReducedMotion = userSettings.Get<bool>(UserSettingsConstants.ReducedMotion);
        TargetFrameRate = userSettings.Get<int>(UserSettingsConstants.TargetFrameRate);
        IsDiagnosticVisible = userSettings.Get<bool>(UserSettingsConstants.DiagnosticsVisible);
        SoundVolume = userSettings.Get<int>(UserSettingsConstants.SoundVolume);
        SelectedMainGraphTypeIndex = (int)userSettings.GetAndDeserialize<GraphType>(UserSettingsConstants.SelectedMainGraphType);

        IsFirstRun = systemInfo.IsFirstRun;
        IsAppUpdated = systemInfo.IsAppUpdated;
        IsHardwareAccelerated = systemInfo.IsHardwareAccelerated;
        logger.LogInformation($"GPU: {systemInfo.GpuName}, Hardware acceleration: {IsHardwareAccelerated}");

        var quality = userSettings.GetAndDeserialize<AppPerformance>(UserSettingsConstants.Performance);
        IsFallbackBackground = quality == AppPerformance.potato || !IsHardwareAccelerated;
        // Alert user only on first run
        IsHardwareAccelerationMissingNotify = !IsHardwareAccelerated && IsFirstRun;

        // Refresh UI/Weather
        weatherRefreshTimer = timerFactory.CreateTimer();
        weatherRefreshTimer.TimerTick += WeatherRefreshTimer_Tick;
        weatherRefreshTimer.Start(new TimeSpan(0, 5, 0));

        // We are not checking last run update checked status to avoid spamming the user with notification.
        appUpdater.UpdateChecked += AppUpdater_UpdateChecked;
        if (!BuildInfoUtil.IsDebugBuild())
            appUpdater.Start();
    }

    public IReadOnlyList<ShaderViewModel> ShaderViewModels { get; private set; }

    [ObservableProperty]
    private ObservableCollection<Location> searchSuggestions = new();

#if WINDOWS_UWP
    [ObservableProperty]
    private AdvancedCollectionView weathersSorted;
#endif

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
                if (value is not null)
                {
                    if (userSettings.GetAndDeserialize<LocationModel>(UserSettingsConstants.SelectedLocation) != value.Location)
                        userSettings.SetAndSerialize(UserSettingsConstants.SelectedLocation, value.Location);
                }
                else
                {
                    // Play default animation if all locations removed
                    SetWeatherAnimation(defaultAnimation);
                }
            }
        }
    }

    ///// <summary>
    ///// Is the user selected location currently daytime, if none selected uses system time.
    ///// </summary>
    public bool IsSelectedLocationDaytime
    {
        get => SelectedWeather?.IsDaytime ?? TimeUtil.IsDaytime();
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

    private int _selectedMainGraphTypeIndex;
    public int SelectedMainGraphTypeIndex
    {
        get => _selectedMainGraphTypeIndex;
        set
        {
            if (value != -1 && userSettings.GetAndDeserialize<GraphType>(UserSettingsConstants.SelectedMainGraphType) != (GraphType)value)
            {
                userSettings.SetAndSerialize(UserSettingsConstants.SelectedMainGraphType, value);
                UpdateGraphs();
            }
            SetProperty(ref _selectedMainGraphTypeIndex, value);
        }
    }

    /// <summary>
    /// Is user idle
    /// </summary>
    [ObservableProperty]
    private bool isIdle;

    [ObservableProperty]
    private bool isAppUpdateAvailable;

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
    private CloudsModel cloudsProperty;

    [ObservableProperty]
    private DepthModel depthProperty;

    [ObservableProperty]
    private RainModel rainProperty;

    [ObservableProperty]
    private SnowModel snowProperty;

    [ObservableProperty]
    private WindModel fogProperty;

    [ObservableProperty]
    private ShaderViewModel selectedShader;

    [ObservableProperty]
    private ShaderQuality selectedShaderQuality;

    [ObservableProperty]
    private bool isPausedShader = false;

    [ObservableProperty]
    private bool isDynamicResolution = true;

    [ObservableProperty]
    private bool isHardwareAccelerated = true;

    [ObservableProperty]
    private int targetFrameRate = 60;

    [ObservableProperty]
    private bool isDiagnosticVisible = false;

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
    private bool isShowAddLocation;

    [ObservableProperty]
    private bool isFetchingLocation = false;

    [ObservableProperty]
    private bool isShowDetectLocation = false;

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
    private async Task RefreshWeather(bool overrideCache)
    {
        try
        {
            CanRefreshCommand = false;
            RefreshWeatherCommand.NotifyCanExecuteChanged();

            if (overrideCache)
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
    private async Task DetectLocation()
    {
        try
        {
            // Get coordinates
            var pos = await geolocationService.GetLocationAsync();
            await SetWeather(null, pos.Latitude, pos.Longitude);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            ErrorMessage = ex.ToString();
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

#if WINDOWS_UWP
        using var deferToken = WeathersSorted.DeferRefresh();
#endif
        // Update SortOrder maximum values
        for (int i = 0; i < Weathers.Count; i++)
        {
            Weathers[i].SortOrder = Weathers[i].SortOrder > obj.SortOrder ?
                Weathers[i].SortOrder - 1 : Weathers[i].SortOrder;
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
            var units = GetWeatherUnits();
            var weatherVm = weatherViewModelFactory.CreateWeatherViewModel(weather,
                airQuality,
                Weathers.Count,
                units,
                userSettings.GetAndDeserialize<GraphType>(UserSettingsConstants.SelectedMainGraphType));
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
            var units = GetWeatherUnits();

            // Set default animation, setting it here instead of constructor to avoid:
            // ComputeSharp/UWP Windowsize issue.
            // Avalonia visual artifacts.
            SetWeatherAnimation(defaultAnimation);

            if (!locations.Any())
                return;

            // Fetch/load the selection first for better UX
            selection ??= locations[0];
            await SetWeather(selection.Name, selection.Latitude, selection.Longitude);

            // Fetch/Load the rest in the background
            var remainingLocations = locations.Where(x => x.Latitude != selection.Latitude && x.Longitude != selection.Longitude);
            foreach (var location in remainingLocations)
            {
                (var weather, var airQuality) = await FetchWeather(location.Name, location.Latitude, location.Longitude);
                Weathers.Add(weatherViewModelFactory.CreateWeatherViewModel(
                    weather,
                    airQuality,
                    0,
                    units,
                    userSettings.GetAndDeserialize<GraphType>(UserSettingsConstants.SelectedMainGraphType)));
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
            // In case cache is outdated, new fetch request takes time.. so to avoid issue if user started spamming it.
            await weatherUpdatingLock.WaitAsync();

            if (!Weathers.Any())
                return;

            IsUpdatingWeather = true;
            var units = GetWeatherUnits();
            var locations = Weathers.OrderBy(x => x.SortOrder).Select(x => x.Location).ToList();
            var selectedLocation = SelectedLocation;
            var selectedDay = SelectedWeather;
            Weathers.Clear();

            for (int i = 0; i < locations.Count; i++)
            {
                // Uses cache if configured and unit convertion takes place in the viewmodel
                (var weather, var airQuality) = await FetchWeather(locations[i].Name, locations[i].Latitude, locations[i].Longitude);
                Weathers.Add(weatherViewModelFactory.CreateWeatherViewModel(weather,
                    airQuality,
                    i,
                    units,
                    userSettings.GetAndDeserialize<GraphType>(UserSettingsConstants.SelectedMainGraphType)));
            }
            SelectedLocation = Weathers.FirstOrDefault(x => x.Location.Latitude == selectedLocation.Location.Latitude && x.Location.Longitude == selectedLocation.Location.Longitude);
            SelectedWeather = SelectedLocation?.Daily?.FirstOrDefault(x => x.ForecastStartTime.Date == selectedDay.ForecastStartTime.Date) ?? SelectedLocation?.Today;
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
            // Null when provider is missing reverse geocoding, GPS location.
            // Use Fetch location since its localized name (if available.) 
            if (!string.IsNullOrEmpty(name))
            {
                weather.Name = name;
                airQuality.Name = name;
            }
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
        if (string.IsNullOrWhiteSpace(search) || search.Length < 2)
            return;

        try
        {
            IsWorking = true;
            IsFetchingLocation = true;
            var locations = await weatherClient.GetLocationDataAsync(search.TrimStart());
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
                    RainProperty.PostProcessing = property.PostProcessing;
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

    private void SetShader(ShaderTypes type)
    {
        // CPU emulation, skip for better UX
        if (!IsHardwareAccelerated)
            return;

        // Turn off all animations
        if (IsFallbackBackground)
        {
            // Turn off shader if running
            SelectedShader = null;

            return;
        }

        // Skip if running already
        if (SelectedShader?.Model?.Type == type)
            return;

        var selection = ShaderViewModels.First(x => x.Model.Type == type);
        SelectedShader = selection;

        // Reset config
        UpdateShaderQuality();
        UpdateBrightness();
        RainProperty.Mouse =
            CloudsProperty.Mouse =
            SnowProperty.Mouse =
            FogProperty.Mouse =
            DepthProperty.Mouse = Vector4.Zero;
    }

    private void UpdateGraphs()
    {
        if (Weathers is null || !Weathers.Any())
            return;

        var graphType = userSettings.GetAndDeserialize<GraphType>(UserSettingsConstants.SelectedMainGraphType);
        foreach (var weatherVm in Weathers)
        {
            var graphs = weatherViewModelFactory.CreateGraphModels(weatherVm, graphType);
            for (int i = 0; i < weatherVm.Daily.Count; i++)
                weatherVm.Daily[i].DayGraph = graphs[i];
        }
    }

    private void UpdateShaderRenderer()
    {
        throw new NotImplementedException();
    }

    private void UpdateQualitySettings()
    {
        var quality = userSettings.GetAndDeserialize<AppPerformance>(UserSettingsConstants.Performance);
        IsFallbackBackground = quality == AppPerformance.potato || !IsHardwareAccelerated;
        SetWeatherAnimation(SelectedWeatherAnimation);
        // Force update since selected shader is unchanged.
        UpdateShaderQuality();
    }

    private void UpdateMotionSettings()
    {
        IsReducedMotion = userSettings.Get<bool>(UserSettingsConstants.ReducedMotion);
        SetWeatherAnimation(SelectedWeatherAnimation);
    }

    private void UpdateShaderQuality()
    {
        if (SelectedShader?.Model is null)
            return;

        switch (userSettings.GetAndDeserialize<AppPerformance>(UserSettingsConstants.Performance))
        {
            case AppPerformance.performance:
                {
                    IsDynamicResolution = false;
                    SelectedShaderQuality = ShaderQuality.optimized;
                }
                break;
            case AppPerformance.quality:
                {
                    IsDynamicResolution = false;
                    SelectedShaderQuality = ShaderQuality.maximum;
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

    private async Task UpdateWeatherProvider()
    {
        var selection = userSettings.GetAndDeserialize<WeatherProviders>(UserSettingsConstants.SelectedWeatherProvider);
        weatherClient = weatherClientFactory.GetInstance(selection);
        IsShowDetectLocation = weatherClient.IsReverseGeocodingSupported;

        if (!weatherClient.IsReverseGeocodingSupported)
        {
            // Location name not be available and can return nearby location which may not be the same.
            ClearLocations();
        }
        else
        {
            // Try to fetch using same (lat, long), can fail if the location not available.
            await UpdateWeather();
        }
    }

    private void ClearLocations()
    {
        Weathers.Clear();
        userSettings.SetAndSerialize<LocationModel>(UserSettingsConstants.SelectedLocation, null);
        userSettings.SetAndSerialize(UserSettingsConstants.PinnedLocations, Array.Empty<LocationModel>());
    }

    private void StoreLocationsSorted()
    {
        var sortedLocations = Weathers.OrderBy(x => x.SortOrder).Select(x => x.Location).ToArray();
        userSettings.SetAndSerialize(UserSettingsConstants.PinnedLocations, sortedLocations);
    }

    private async void WeatherRefreshTimer_Tick(object sender, EventArgs e)
    {
        // Since Screensaver mode allows custom weather animation selection, do not update.
        // If not idle to avoid user distraction, do not update.
        if (!IsMainPage || !IsIdle)
            return;

        // Cheap way of refreshing data and visuals.
        await RefreshWeather(false);
    }

    private void AppUpdater_UpdateChecked(object sender, AppUpdateStatus e)
    {
        logger.LogInformation($"Application update checked: {e}");
        IsAppUpdateAvailable = e == AppUpdateStatus.available;
    }

    private WeatherUnitSettings GetWeatherUnits()
    {
        var units = userSettings.GetAndDeserialize<WeatherUnits>(UserSettingsConstants.WeatherUnit);
        switch (units)
        {
            case WeatherUnits.metric:
            case WeatherUnits.imperial:
            case WeatherUnits.hybrid:
                return new WeatherUnitSettings(units);
            case WeatherUnits.custom:
                {
                    var temperatureUnit = userSettings.GetAndDeserialize<TemperatureUnits>(UserSettingsConstants.SelectedTemperatureUnit);
                    var windSpeedUnit = userSettings.GetAndDeserialize<WindSpeedUnits>(UserSettingsConstants.SelectedWindSpeedUnit);
                    var visibilityUnit = userSettings.GetAndDeserialize<VisibilityUnits>(UserSettingsConstants.SelectedVisibilityUnit);
                    var pressureUnit = userSettings.GetAndDeserialize<PressureUnits>(UserSettingsConstants.SelectedPressureUnit);
                    var precipitationUnit = userSettings.GetAndDeserialize<PrecipitationUnits>(UserSettingsConstants.SelectedPrecipitationUnit);
                    return new WeatherUnitSettings(temperatureUnit, windSpeedUnit, visibilityUnit, pressureUnit, precipitationUnit);
                }
            default:
                throw new NotImplementedException();
        }
    }
}
