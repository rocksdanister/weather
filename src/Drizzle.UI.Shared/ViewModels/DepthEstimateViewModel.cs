using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drizzle.Common.Helpers;
using Drizzle.Common.Services;
using Drizzle.ImageProcessing;
using Drizzle.ML.DepthEstimate;
using Drizzle.Models.Enums;
using Drizzle.Models.Shaders;
using Drizzle.UI.Shared.Factories;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Drizzle.UI.Shared.ViewModels;

public partial class DepthEstimateViewModel : ObservableObject
{
    private readonly IDepthEstimate depthEstimate;
    private readonly IDownloadService downloader;
    private readonly IFileService fileService;

    public string DepthAssetDir { get; private set; }
    public event EventHandler OnRequestClose;

    private readonly Uri modelUri = new("https://github.com/rocksdanister/lively-ml-models/releases/download/v1.0.0.0/model.onnx");
    private CancellationTokenSource downloadCts;
    private readonly string modelPath;

    public DepthEstimateViewModel(IShaderViewModelFactory shaderViewModelFactory,
        IDepthEstimate depthEstimate,
        IDownloadService downloader,
        IFileService fileService)
    {
        this.depthEstimate = depthEstimate;
        this.downloader = downloader;
        this.fileService = fileService;

        this.modelPath = Path.Combine(fileService.LocalFolderPath, "ML", "Midas", "model.onnx");
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

    [RelayCommand]
    private async Task AddImage()
    {
        var (stream, fileName) = await fileService.OpenImageFileAsync();
        if (stream is null)
            return;

        var tempFilePath = Path.Combine(fileService.TempFolderPath, fileName);
        tempFilePath = FileUtil.NextAvailableFilename(tempFilePath);
        using (stream)
        {
            using var fileStream = File.Create(tempFilePath);
            await stream.CopyToAsync(fileStream);
        }

        // Resizing with blur to avoid aliasing on images with details.
        var shaderTexturePath = FileUtil.NextAvailableFilename(tempFilePath);
        await Task.Run(() => ImageUtil.GaussianBlur(tempFilePath, shaderTexturePath, 1, 800));
        SelectedShaderProperties.ImagePath = shaderTexturePath;

        SelectedImage = tempFilePath;
        CanRunCommand = IsModelExists && SelectedImage is not null;
        RunCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanRunCommand))]
    private async Task Run()
    {
        if (SelectedImage is null)
            return;

        var destinationFolder = Path.Combine(fileService.LocalFolderPath, "Backgrounds", "Depth", Path.GetRandomFileName());
        var depthImagePath = Path.Combine(destinationFolder, "depth.jpg");
        var inputImageCopyPath = Path.Combine(destinationFolder, "image.jpg");

        try
        {
            IsRunning = true;
            CanRunCommand = false;
            RunCommand.NotifyCanExecuteChanged();
            CanCancelCommand = false;
            CancelCommand.NotifyCanExecuteChanged();

            Directory.CreateDirectory(destinationFolder);

            await Task.Run(async () =>
            {
                using var inputImage = Image.Load(SelectedImage);
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
                var depthOutput = depthEstimate.Run(SelectedImage);
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

            DepthAssetDir = destinationFolder;
            // Close dialog
            OnRequestClose?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorText = $"Error: {ex.Message}";
            try
            {
                Directory.Delete(destinationFolder, true);
            }
            catch { /* Nothing to do */ }
        }
        finally
        {
            IsRunning = false;
            CanCancelCommand = true;
            CancelCommand.NotifyCanExecuteChanged();
        }
    }


    [RelayCommand(CanExecute = nameof(CanCancelCommand))]
    private void Cancel()
    {
        downloadCts?.Cancel();
        downloadCts = null;
    }

    [RelayCommand(CanExecute = nameof(CanDownloadModelCommand))]
    private async Task DownloadModel()
    {
        try
        {
            CanDownloadModelCommand = false;
            DownloadModelCommand.NotifyCanExecuteChanged();

            var downloadPath = modelPath;
            downloadCts = new CancellationTokenSource();

            await downloader.DownloadFile(modelUri, downloadPath, new Progress<(double downloaded, double total)>(progress =>
            {
                ModelDownloadProgressText = $"{progress.downloaded}/{progress.total} MB";
                ModelDownloadProgress = (float)(progress.downloaded * 100 / progress.total);
            }), downloadCts.Token);

            IsModelExists = CheckModel();
            BackgroundImage = IsModelExists ? SelectedImage : BackgroundImage;

            CanRunCommand = IsModelExists;
            RunCommand.NotifyCanExecuteChanged();
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
    }

    public void OnClose()
    {
        SelectedShader = null;

        try
        {
            if (SelectedImage is not null)
                File.Delete(SelectedImage);

            if (SelectedShaderProperties?.ImagePath is not null)
                File.Delete(SelectedShaderProperties.ImagePath);
        }
        catch { /* Tempfolder, ignore. */ }
    }

    private bool CheckModel() => File.Exists(modelPath);
}
