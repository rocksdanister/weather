using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drizzle.Common.Constants;
using Drizzle.Common.Helpers;
using Drizzle.Common.Services;
using Drizzle.ImageProcessing;
using Drizzle.Models;
using Drizzle.Models.Enums;
using Drizzle.Models.Weather;
using Drizzle.UI.Shared.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Drizzle.UI.Shared.ViewModels;

public sealed partial class ScreensaverViewModel : ObservableObject
{
    private readonly ILogger logger;
    private readonly INavigator navigator;
    private readonly IDialogService dialogService;
    private readonly IAssetReader assetReader;
    private readonly IUserSettings userSettings;
    private readonly IFileService fileService;

    private readonly WmoWeatherCode defaultWeatherAnimation;

    public ScreensaverViewModel(ShellViewModel shellVm,
        INavigator navigator,
        ILogger<ScreensaverViewModel> logger,
        IAssetReader assetReader,
        IDialogService dialogService,
        IUserSettings userSettings,
        IFileService fileService)
    {
        this.ShellVm = shellVm;
        this.navigator = navigator;
        this.dialogService = dialogService;
        this.assetReader = assetReader;
        this.userSettings = userSettings;
        this.fileService = fileService;
        this.logger = logger;

        // Mouse drag effect only in screensaver mode
        ShellVm.IsMouseDrag = true;
        defaultWeatherAnimation = this.ShellVm.SelectedWeatherAnimation;

        UpdateWeatherSelection();
        ShellVm.PropertyChanged += ShellVm_PropertyChanged;

        foreach (int i in Enum.GetValues(typeof(WmoWeatherCode)))
        {
            Weathers.Add(new ScreensaverModel(i, shellVm.IsSelectedLocationDaytime));
        }
        SelectedWeather = Weathers.FirstOrDefault(x => x.WeatherCode == (int)ShellVm.SelectedWeatherAnimation);

        AutoHideMenu = userSettings.Get<bool>(UserSettingsConstants.AutoHideScreensaverMenu);

        LoadDefaultBackgrounds();
        LoadUserBackgrounds();
        UpdateBackgroundSelection();
    }

    [ObservableProperty]
    private ObservableCollection<ScreensaverModel> weathers = new();

    private ScreensaverModel _selectedWeather;
    public ScreensaverModel SelectedWeather
    {
        get => _selectedWeather;
        set
        {
            if (value.WeatherCode != (int)ShellVm.SelectedWeatherAnimation)
            {
                // Change random background if switching from different shader type for better UX (shader image change have no transition)
                var type1 = ((WmoWeatherCode)_selectedWeather.WeatherCode).GetWeather().Type;
                var type2 = ((WmoWeatherCode)value.WeatherCode).GetWeather().Type;
                ShellVm.SetWeatherAnimation((WmoWeatherCode)value.WeatherCode, type1 != type2);
            }
            SetProperty(ref _selectedWeather, value);
        }
    }

    private bool _autoHideMenu;
    public bool AutoHideMenu
    {
        get => _autoHideMenu;
        set
        {
            if (userSettings.Get<bool>(UserSettingsConstants.AutoHideScreensaverMenu) != value)
                userSettings.Set(UserSettingsConstants.AutoHideScreensaverMenu, value);

            SetProperty(ref _autoHideMenu, value);
        }
    }

    [ObservableProperty]
    private bool isFullScreen;

    [ObservableProperty]
    private bool isRainPropertyVisible;

    [ObservableProperty]
    private bool isSnowPropertyVisible;

    [ObservableProperty]
    private bool isCloudsPropertyVisible;

    [ObservableProperty]
    private bool isDepthPropertyVisible;

    [ObservableProperty]
    private bool isFogPropertyVisible;

    [ObservableProperty]
    private bool isBusy;

    public ShellViewModel ShellVm { get; }

    [RelayCommand]
    private void Restore()
    {
        ShellVm.SetWeatherAnimation(ShellVm.SelectedWeatherAnimation, false);
    }

    [RelayCommand]
    private void GoBack()
    {
        if (IsFullScreen)
            navigator.ToFullscreen(false);

        navigator.NavigateTo(ContentPageType.Main);

        // Useful for testing/demo, on release prevent user selection outside screensaver page.
        if (!BuildInfoUtil.IsDebugBuild())
        {
            // Reset all backgrounds to stock
            ShellVm.RandomizeWeatherBackgrounds();
            ShellVm.SetWeatherAnimation(defaultWeatherAnimation, false);
        }
        else
        {
            // Restore weather, keep background for testing
            ShellVm.SetWeatherAnimation(defaultWeatherAnimation, false);
            logger.LogDebug("Navigating to mainpage while maintaining selected backgrounds.");
        }

        ShellVm.IsMouseDrag = false;
        ShellVm.PropertyChanged -= ShellVm_PropertyChanged;
    }

    [RelayCommand]
    private void FullScreen()
    {
        IsFullScreen = navigator.ToFullscreen(!IsFullScreen);
    }

    private void ShellVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "SelectedWeatherAnimation")
        {
            UpdateWeatherSelection();
            UpdateBackgroundSelection();
        }
    }

    private void UpdateWeatherSelection()
    {
        IsRainPropertyVisible = IsSnowPropertyVisible = IsCloudsPropertyVisible = IsFogPropertyVisible = IsDepthPropertyVisible = false;
        switch (ShellVm.SelectedWeatherAnimation.GetShader())
        {
            case ShaderTypes.clouds:
                IsCloudsPropertyVisible = true;
                break;
            case ShaderTypes.rain:
                IsRainPropertyVisible = true;
                break;
            case ShaderTypes.snow:
                IsSnowPropertyVisible = true;
                break;
            case ShaderTypes.depth:
                IsDepthPropertyVisible = true;
                break;
            case ShaderTypes.fog:
                IsFogPropertyVisible = true;
                break;
        }
    }

    #region background selection

    [ObservableProperty]
    private ObservableCollection<UserImageModel> rainBackgrounds = new();

    [ObservableProperty]
    private ObservableCollection<UserImageModel> snowBackgrounds = new();

    [ObservableProperty]
    private ObservableCollection<UserImageModel> depthBackgrounds = new();

    private UserImageModel _selectedRainBackground;
    public UserImageModel SelectedRainBackground
    {
        get => _selectedRainBackground;
        set
        {
            if (value is not null && ShellVm.RainProperty.ImagePath != value.Image)
            {
                ShellVm.RainProperty.ImagePath = value.Image;
            }
            SetProperty(ref _selectedRainBackground, value);
        }
    }

    private UserImageModel _selectedSnowBackground;
    public UserImageModel SelectedSnowBackground
    {
        get => _selectedSnowBackground;
        set
        {
            if (value is not null && ShellVm.SnowProperty.ImagePath != value.Image)
            {
                ShellVm.SnowProperty.ImagePath = value.Image;
            }
            SetProperty(ref _selectedSnowBackground, value);
        }
    }

    private UserImageModel _selectedDepthBackground;
    public UserImageModel SelectedDepthBackground
    {
        get => _selectedDepthBackground;
        set
        {
            if (value is not null && ShellVm.DepthProperty.ImagePath != value.Image)
            {
                ShellVm.DepthProperty.ImagePath = value.Image;
                ShellVm.DepthProperty.DepthPath = value.Depth;
            }
            SetProperty(ref _selectedDepthBackground, value);
        }
    }


    private UserImageModel _selectedFogBackground;
    public UserImageModel SelectedFogBackground
    {
        get => _selectedFogBackground;
        set
        {
            if (value is not null && ShellVm.FogProperty.ImagePath != value.Image)
            {
                ShellVm.FogProperty.ImagePath = value.Image;
                ShellVm.FogProperty.DepthPath = value.Depth;
            }
            SetProperty(ref _selectedFogBackground, value);
        }
    }

    [RelayCommand]
    private async Task ChangeBackground()
    {
        IsBusy = true;
        switch (((WmoWeatherCode)SelectedWeather.WeatherCode).GetShader())
        {
            case ShaderTypes.rain:
                {
                    var (stream, name) = await fileService.OpenImageFileAsync();
                    if (stream is not null)
                    {
                        var imagePath = Path.Combine(fileService.LocalFolderPath, "Backgrounds", "Rain", name);
                        imagePath = FileUtil.NextAvailableFilename(imagePath);
                        using (stream)
                        {
                            ImageUtil.GaussianBlur(stream, imagePath, 12, 1920);
                        }

                        var selection = new UserImageModel(name, imagePath, null, DateTime.Now, true);
                        RainBackgrounds.Add(selection);
                        SelectedRainBackground = selection;
                    }
                }
                break;
            case ShaderTypes.snow:
                {
                    var (stream, name) = await fileService.OpenImageFileAsync();
                    if (stream is not null)
                    {
                        var imagePath = Path.Combine(fileService.LocalFolderPath, "Backgrounds", "Snow", name);
                        imagePath = FileUtil.NextAvailableFilename(imagePath);
                        using (stream)
                        {
                            ImageUtil.GaussianBlur(stream, imagePath, 12, 1920);
                        }

                        var selection = new UserImageModel(name, imagePath, null, DateTime.Now, true);
                        SnowBackgrounds.Add(selection);
                        SelectedSnowBackground = selection;
                    }
                }
                break;
            case ShaderTypes.depth:
                {
                    var path = await dialogService.ShowDepthCreationDialogAsync();
                    if (path is not null)
                    {
                        var selection = new UserImageModel("", Path.Combine(path, "image.jpg"), Path.Combine(path, "depth.jpg"), DateTime.Now, true);
                        DepthBackgrounds.Add(selection);
                        SelectedDepthBackground = selection;
                    }
                }
                break;
            case ShaderTypes.fog:
                {
                    var path = await dialogService.ShowDepthCreationDialogAsync();
                    if (path is not null)
                    {
                        var selection = new UserImageModel("", Path.Combine(path, "image.jpg"), Path.Combine(path, "depth.jpg"), DateTime.Now, true);
                        // Share same directory of files
                        DepthBackgrounds.Add(selection);
                        SelectedFogBackground = selection;
                    }
                }
                break;
            case ShaderTypes.clouds:
            case ShaderTypes.tunnel:
            default:
                break;
        }
        IsBusy = false;
    }

    [RelayCommand]
    private void DeleteBackground(UserImageModel obj)
    {
        switch (((WmoWeatherCode)SelectedWeather.WeatherCode).GetShader())
        {
            case ShaderTypes.rain:
                if (obj is not null && obj.IsEditable)
                {
                    SelectedRainBackground = SelectedRainBackground != obj ? SelectedRainBackground : RainBackgrounds[0];
                    RainBackgrounds.Remove(obj);
                    File.Delete(obj.Image);
                }
                break;
            case ShaderTypes.snow:
                if (obj is not null && obj.IsEditable)
                {
                    SelectedSnowBackground = SelectedSnowBackground != obj ? SelectedSnowBackground : SnowBackgrounds[0];
                    SnowBackgrounds.Remove(obj);
                    File.Delete(obj.Image);
                }
                break;
            case ShaderTypes.depth:
                if (obj is not null && obj.IsEditable)
                {
                    SelectedDepthBackground = SelectedDepthBackground != obj ? SelectedDepthBackground : DepthBackgrounds[0];
                    DepthBackgrounds.Remove(obj);
                    Directory.Delete(Path.GetDirectoryName(obj.Image), true);
                }
                break;
            case ShaderTypes.fog:
                if (obj is not null && obj.IsEditable)
                {
                    // Shared with depth backgrounds
                    SelectedFogBackground = SelectedFogBackground != obj ? SelectedFogBackground : DepthBackgrounds[0];
                    DepthBackgrounds.Remove(obj);
                    Directory.Delete(Path.GetDirectoryName(obj.Image), true);
                }
                break;
            case ShaderTypes.clouds:
            case ShaderTypes.tunnel:
            default:
                break;
        }
    }

    private void LoadDefaultBackgrounds()
    {
        foreach (var item in assetReader.GetBackgrounds(ShaderTypes.rain))
        {
            RainBackgrounds.Add(new(item.Name, item.FilePath, item.DepthPath, item.Time[0], false));
        }
        foreach (var item in assetReader.GetBackgrounds(ShaderTypes.snow))
        {
            SnowBackgrounds.Add(new(item.Name, item.FilePath, item.DepthPath, item.Time[0], false));
        }
        foreach (var item in assetReader.GetBackgrounds(ShaderTypes.depth))
        {
            DepthBackgrounds.Add(new(item.Name, item.FilePath, item.DepthPath, item.Time[0], false));
        }
    }

    private void LoadUserBackgrounds()
    {
        var userBackgroundsRain = Path.Combine(fileService.LocalFolderPath, "Backgrounds", "Rain");
        var userBackgroundsSnow = Path.Combine(fileService.LocalFolderPath, "Backgrounds", "Snow");
        var userBackgroundsDepth = Path.Combine(fileService.LocalFolderPath, "Backgrounds", "Depth");
        // Create if does not exists
        Directory.CreateDirectory(userBackgroundsRain);
        Directory.CreateDirectory(userBackgroundsSnow);
        Directory.CreateDirectory(userBackgroundsDepth);

        foreach (var filePath in Directory.GetFiles(userBackgroundsRain))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            RainBackgrounds.Add(new(fileName, filePath, null, DateTime.Now, true));
        }
        foreach (var filePath in Directory.GetFiles(userBackgroundsSnow))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            SnowBackgrounds.Add(new(fileName, filePath, null, DateTime.Now, true));
        }
        foreach (var filePath in Directory.GetDirectories(userBackgroundsDepth))
        {
            DepthBackgrounds.Add(new("", Path.Combine(filePath, "image.jpg"), Path.Combine(filePath, "depth.jpg"), DateTime.Now, true));
        }
    }

    private void UpdateBackgroundSelection()
    {
        SelectedRainBackground = RainBackgrounds.FirstOrDefault(x => x.Image == ShellVm.RainProperty.ImagePath);
        SelectedSnowBackground = SnowBackgrounds.FirstOrDefault(x => x.Image == ShellVm.SnowProperty.ImagePath);
        SelectedDepthBackground = DepthBackgrounds.FirstOrDefault(x => x.Image == ShellVm.DepthProperty.ImagePath);
        SelectedFogBackground = DepthBackgrounds.FirstOrDefault(x => x.Image == ShellVm.FogProperty.ImagePath);
    }

    #endregion //background selection
}
