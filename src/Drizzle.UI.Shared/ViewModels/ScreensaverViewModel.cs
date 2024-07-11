using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drizzle.Common;
using Drizzle.Common.Services;
using Drizzle.Models;
using Drizzle.UI.Shared.Shaders.Helpers;
using Drizzle.UI.UWP.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Drizzle.ImageProcessing;
using Drizzle.Common.Helpers;
using Drizzle.Common.Constants;
using Windows.UI.Xaml.Controls.Primitives;
using Newtonsoft.Json.Linq;
using Drizzle.Common.Extensions;
using Microsoft.Extensions.Logging;
using Drizzle.Weather.Helpers;
using Drizzle.Models.Weather;
using Drizzle.Models.Enums;




#if WINDOWS_UWP
using Windows.Storage.Pickers;
using Windows.Storage;
#endif

namespace Drizzle.UI.UWP.ViewModels
{
    public sealed partial class ScreensaverViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly INavigator navigator;
        private readonly IDialogService dialogService;
        private readonly IAssetReader assetReader;
        private readonly IUserSettings userSettings;
        private readonly WmoWeatherCode defaultWeatherAnimation;
        private readonly bool isResetBackgroundsOnExit;

        public ScreensaverViewModel(ShellViewModel shellVm,
            INavigator navigator,
            ILogger<ScreensaverViewModel> logger,
            IAssetReader assetReader,
            IDialogService dialogService,
            IUserSettings userSettings)
        {
            this.ShellVm = shellVm;
            this.navigator = navigator;
            this.dialogService = dialogService;
            this.assetReader = assetReader;
            this.userSettings = userSettings;
            this.logger = logger;

            // Useful for testing/demo, on release prevent user selection outside screensaver page.
#if DEBUG
            isResetBackgroundsOnExit = false;
#else
            isResetBackgroundsOnExit = true;
#endif

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

            InitializeBackgrounds().Await(() => { }, (ex) => logger.LogError(ex.ToString()));
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

            if (isResetBackgroundsOnExit)
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
                    ShellVm.DepthProperty.DepthPath = Path.Combine(Path.GetDirectoryName(value.Image), "depth.jpg");
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
                    ShellVm.FogProperty.DepthPath = Path.Combine(Path.GetDirectoryName(value.Image), "depth.jpg");
                }
                SetProperty(ref _selectedFogBackground, value);
            }
        }

        [RelayCommand]
        private async Task RainBackgroundChange()
        {
            IsBusy = true;
            var file = await ShowImageDialog();
            if (file is not null)
            {
                var imagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Backgrounds", "Rain", file.Name);
                imagePath = FileUtil.NextAvailableFilename(imagePath);
                using var stream = await file.OpenStreamForReadAsync();
                ImageUtil.GaussianBlur(stream, imagePath, 12, 1920);

                var selection = new UserImageModel(file.DisplayName, imagePath, DateTime.Now, true);
                RainBackgrounds.Add(selection);
                SelectedRainBackground = selection;
            }
            IsBusy = false;
        }

        [RelayCommand]
        private async Task SnowBackgroundChange()
        {
            IsBusy = true;
            var file = await ShowImageDialog();
            if (file is not null)
            {
                var imagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Backgrounds", "Snow", file.Name);
                imagePath = FileUtil.NextAvailableFilename(imagePath);
                using var stream = await file.OpenStreamForReadAsync();
                ImageUtil.GaussianBlur(stream, imagePath, 12, 1920);

                var selection = new UserImageModel(file.DisplayName, imagePath, DateTime.Now, true);
                SnowBackgrounds.Add(selection);
                SelectedSnowBackground = selection;
            }
            IsBusy = false;
        }

        [RelayCommand]
        private async Task DepthBackgroundChange()
        {
            var path = await dialogService.ShowDepthCreationDialogAsync();
            if (path is not null)
            {
                var selection = new UserImageModel("", Path.Combine(path, "image.jpg"), DateTime.Now, true);
                DepthBackgrounds.Add(selection);
                SelectedDepthBackground = selection;
            }
        }

        [RelayCommand]
        private async Task FogBackgroundChange()
        {
            var path = await dialogService.ShowDepthCreationDialogAsync();
            if (path is not null)
            {
                var selection = new UserImageModel("", Path.Combine(path, "image.jpg"), DateTime.Now, true);
                // Share same directory of files
                DepthBackgrounds.Add(selection);
                SelectedFogBackground = selection;
            }
        }

        [RelayCommand]
        private async Task RainBackgroundDelete(UserImageModel obj)
        {
            if (obj is not null && obj.IsEditable)
            {
                SelectedRainBackground = SelectedRainBackground != obj ? SelectedRainBackground : RainBackgrounds[0];
                RainBackgrounds.Remove(obj);
                await (await StorageFile.GetFileFromPathAsync(obj.Image)).DeleteAsync();
            }
        }

        [RelayCommand]
        private async Task SnowBackgroundDelete(UserImageModel obj)
        {
            if (obj is not null && obj.IsEditable)
            {
                SelectedSnowBackground = SelectedSnowBackground != obj ? SelectedSnowBackground : SnowBackgrounds[0];
                SnowBackgrounds.Remove(obj);
                await (await StorageFile.GetFileFromPathAsync(obj.Image)).DeleteAsync();
            }
        }

        [RelayCommand]
        private async Task DepthBackgroundDelete(UserImageModel obj)
        {
            if (obj is not null && obj.IsEditable)
            {
                SelectedDepthBackground = SelectedDepthBackground != obj ? SelectedDepthBackground : DepthBackgrounds[0];
                DepthBackgrounds.Remove(obj);
                await (await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(obj.Image))).DeleteAsync();
            }
        }

        [RelayCommand]
        private async Task FogBackgroundDelete(UserImageModel obj)
        {
            if (obj is not null && obj.IsEditable)
            {
                // Shared with depth backgrounds
                SelectedFogBackground = SelectedFogBackground != obj ? SelectedFogBackground : DepthBackgrounds[0];
                DepthBackgrounds.Remove(obj);
                await (await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(obj.Image))).DeleteAsync();
            }
        }

        private async Task InitializeBackgrounds()
        {
            // Defaults
            foreach (var item in assetReader.GetBackgrounds(ShaderTypes.rain))
            {
                RainBackgrounds.Add(new(item.Name, item.FilePath, item.Time[0], false));
            }
            foreach (var item in assetReader.GetBackgrounds(ShaderTypes.snow))
            {
                SnowBackgrounds.Add(new(item.Name, item.FilePath, item.Time[0], false));
            }
            foreach (var item in assetReader.GetBackgrounds(ShaderTypes.depth))
            {
                DepthBackgrounds.Add(new(item.Name, item.FilePath, item.Time[0], false));
            }

            // Create cache directory
            var localFolder = ApplicationData.Current.LocalFolder;
            var cacheFolder = await localFolder.CreateFolderAsync("Backgrounds", CreationCollisionOption.OpenIfExists);
            var rainCacheFolder = await cacheFolder.CreateFolderAsync("Rain", CreationCollisionOption.OpenIfExists);
            var snowCacheFolder = await cacheFolder.CreateFolderAsync("Snow", CreationCollisionOption.OpenIfExists);
            var depthCacheFolder = await cacheFolder.CreateFolderAsync("Depth", CreationCollisionOption.OpenIfExists);

            // Populate cache
            foreach (var item in await rainCacheFolder.GetFilesAsync())
            {
                RainBackgrounds.Add(new(item.DisplayName, item.Path, DateTime.Now, true));
            }
            foreach (var item in await snowCacheFolder.GetFilesAsync())
            {
                SnowBackgrounds.Add(new(item.DisplayName, item.Path, DateTime.Now, true));
            }
            foreach (var item in await depthCacheFolder.GetFoldersAsync())
            {
                DepthBackgrounds.Add(new("", Path.Combine(item.Path, "image.jpg"), DateTime.Now, true));
            }

            // Current selection
            UpdateBackgroundSelection();
        }

        private void UpdateBackgroundSelection()
        {
            SelectedRainBackground = RainBackgrounds.FirstOrDefault(x => x.Image == ShellVm.RainProperty.ImagePath);
            SelectedSnowBackground = SnowBackgrounds.FirstOrDefault(x => x.Image == ShellVm.SnowProperty.ImagePath);
            SelectedDepthBackground = DepthBackgrounds.FirstOrDefault(x => x.Image == ShellVm.DepthProperty.ImagePath);
            SelectedFogBackground = DepthBackgrounds.FirstOrDefault(x => x.Image == ShellVm.FogProperty.ImagePath);
        }

        private async Task<StorageFile> ShowImageDialog()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            return await picker.PickSingleFileAsync();
        }

        #endregion //background selection
    }
}
