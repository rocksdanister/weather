using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drizzle.Common.Helpers;
using Drizzle.ImageProcessing;
using Drizzle.ML.DepthEstimate;
using Drizzle.Models.Enums;
using Drizzle.Models.Shaders;
using Drizzle.UI.Shared.Factories;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;

#if WINDOWS_UWP
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
#endif

namespace Drizzle.UI.Shared.ViewModels;

public partial class DepthEstimateViewModel : ObservableObject
{
    private readonly IShaderViewModelFactory shaderViewModelFactory;
    private readonly IDepthEstimate depthEstimate;
    private readonly IDownloadUtil downloader;

    public string DepthAssetDir { get; private set; }
    public event EventHandler OnRequestClose;

    private readonly string modelPath;

    public DepthEstimateViewModel(IShaderViewModelFactory shaderViewModelFactory, IDepthEstimate depthEstimate, IDownloadUtil downloader)
    {
        this.shaderViewModelFactory = shaderViewModelFactory;
        this.depthEstimate = depthEstimate;
        this.downloader = downloader;

        // TODO: Cross-platform
#if WINDOWS_UWP
        modelPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "ML", "midas", "model.onnx");
#endif

        IsModelExists = CheckModel();
        CanRunCommand = IsModelExists && SelectedImage is not null;
        RunCommand.NotifyCanExecuteChanged();
        var tunnelVm = shaderViewModelFactory.Create(ShaderTypes.tunnel);
        SelectedShaderProperties = tunnelVm.Model as TunnelModel;
        SelectedShaderProperties.Brightness = 0.95f;
        SelectedShaderProperties.IsSquare = true;
        SelectedShaderProperties.Speed = 0.1f;
        SelectedShader = tunnelVm;
    }

    [ObservableProperty]
    private ShaderViewModel selectedShader;

    [ObservableProperty]
    private TunnelModel selectedShaderProperties;

    [ObservableProperty]
    private bool isModelExists;

    [ObservableProperty]
    private bool isRunning;

    [ObservableProperty]
    private string errorText;

    [ObservableProperty]
    private string backgroundImage;

    [ObservableProperty]
    private string previewImage;

    [ObservableProperty]
    private float modelDownloadProgress;

    [ObservableProperty]
    private string modelDownloadProgressText = "--/-- MB";

    private string _selectedImage;
    public string SelectedImage
    {
        get => _selectedImage;
        set
        {
            SetProperty(ref _selectedImage, value);
            BackgroundImage = value;
            PreviewImage = value;
        }
    }

    private bool CanRunCommand { get; set; } = false;

    private bool CanCancelCommand { get; set; } = true;

    private bool CanDownloadModelCommand { get; set; } = true;

    [RelayCommand(CanExecute = nameof(CanRunCommand))]
    private async Task Run()
    {
        // TODO: Cross-platform
#if WINDOWS_UWP
        var localFolder = ApplicationData.Current.LocalFolder;
        var cacheFolder = await localFolder.CreateFolderAsync("Backgrounds", CreationCollisionOption.OpenIfExists);
        var depthCacheFolder = await cacheFolder.CreateFolderAsync("Depth", CreationCollisionOption.OpenIfExists);
        var destinationFolder = await depthCacheFolder.CreateFolderAsync(Path.GetFileNameWithoutExtension(SelectedImage), CreationCollisionOption.GenerateUniqueName);
        var inputImageFile = await StorageFile.GetFileFromPathAsync(SelectedImage);
        var depthImagePath = Path.Combine(destinationFolder.Path, "depth.jpg");
        var inputImageCopyPath = Path.Combine(destinationFolder.Path, "image.jpg");

        try
        {
            IsRunning = true;
            CanRunCommand = false;
            RunCommand.NotifyCanExecuteChanged();
            CanCancelCommand = false;
            CancelCommand.NotifyCanExecuteChanged();

            await Task.Run(async () =>
            {
                using var inputImage = Image.Load(inputImageFile.Path);
                //Resize input for performance and memory
                if (inputImage.Width > 3840 || inputImage.Height > 3840)
                {
                    //Fit the image within aspect ratio, if width > height = 3840x.. else ..x3840
                    inputImage.Mutate(x =>
                    {
                        x.Resize(new ResizeOptions()
                        {
                            Size = new Size(3840, 3840),
                            Mode = ResizeMode.Max
                        });
                    });
                }

                if (!modelPath.Equals(depthEstimate.ModelPath, StringComparison.Ordinal))
                    depthEstimate.LoadModel(modelPath);
                var depthOutput = depthEstimate.Run(inputImageFile.Path);
                //Resize depth to same size as input
                using var depthImage = ImageUtil.FloatArrayToImage(depthOutput.Depth, depthOutput.Width, depthOutput.Height);
                depthImage.Mutate(x =>
                {
                    x.Resize(new ResizeOptions()
                    {
                        Mode = ResizeMode.Stretch,
                        Size = new Size(inputImage.Width, inputImage.Height)
                    });
                });
                await depthImage.SaveAsJpegAsync(depthImagePath);
                await inputImage.SaveAsJpegAsync(inputImageCopyPath);
            });

            // Preview output to user
            await Task.Delay(500);
            PreviewImage = depthImagePath;
            await Task.Delay(1500);

            DepthAssetDir = destinationFolder.Path;
            // Close dialog
            OnRequestClose?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorText = $"Error: {ex.Message}";
            await destinationFolder.DeleteAsync();
        }
        finally
        {
            IsRunning = false;
            CanCancelCommand = true;
            CancelCommand.NotifyCanExecuteChanged();
        }
#endif
    }


    [RelayCommand(CanExecute = nameof(CanCancelCommand))]
    private void Cancel()
    {
        downloader?.Cancel();
    }

    [RelayCommand(CanExecute = nameof(CanDownloadModelCommand))]
    private async Task DownloadModel()
    {
        // TODO: Cross-platform
#if WINDOWS_UWP
        try
        {
            CanDownloadModelCommand = false;
            DownloadModelCommand.NotifyCanExecuteChanged();

            var uri = new Uri("https://github.com/rocksdanister/lively-ml-models/releases/download/v1.0.0.0/model.onnx");
            var localFolder = ApplicationData.Current.LocalFolder;
            var machineLearningFolder = await localFolder.CreateFolderAsync("ML", CreationCollisionOption.OpenIfExists);
            var midasFolder = await machineLearningFolder.CreateFolderAsync("Midas", CreationCollisionOption.OpenIfExists);
            var downloadPath = Path.Combine(midasFolder.Path, "model.onnx");
            downloader.DownloadProgressChanged += async(s, e) =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ModelDownloadProgressText = $"{e.DownloadedSize}/{e.TotalSize} MB";
                    ModelDownloadProgress = (float)e.Percentage;
                });
            };
            downloader.DownloadFileCompleted += async(s, success) =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (success)
                    {                 
                        IsModelExists = CheckModel();
                        BackgroundImage = IsModelExists ? SelectedImage : BackgroundImage;

                        CanRunCommand = IsModelExists;
                        RunCommand.NotifyCanExecuteChanged();
                    }
                    else
                    {
                        ErrorText = $"Error: Download failed.";
                    }
                });
            };

            await downloader.DownloadFile(uri, downloadPath);
        }
        catch (Exception ex)
        {
            ErrorText = $"Error: {ex.Message}";
        }
        //finally
        //{
        //    _canDownloadModelCommand = true;
        //    DownloadModelCommand.NotifyCanExecuteChanged();
        //}
#endif
    }

    public async Task OnClose()
    {
        SelectedShader = null;
        // TODO: Cross-platform
#if WINDOWS_UWP
        try
        {
            if (SelectedImage is not null)
                await (await StorageFile.GetFileFromPathAsync(SelectedImage)).DeleteAsync();

            if (SelectedShaderProperties?.ImagePath is not null)
                await (await StorageFile.GetFileFromPathAsync(SelectedShaderProperties.ImagePath)).DeleteAsync();
        }
        catch { /* Tempfolder, ignore. */ }
#endif
    }

    private bool CheckModel() => File.Exists(modelPath);
}
