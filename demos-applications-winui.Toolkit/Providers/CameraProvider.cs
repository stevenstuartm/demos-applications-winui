using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Windows.Media.Capture;
using WinRT.Interop;
using demos_applications_winui.Toolkit.Platform;
using CapturedPhoto = demos_applications_winui.Toolkit.Platform.CapturedPhoto;

namespace demos_applications_winui.Toolkit.Providers;

public partial class CameraProvider(
    IWindowHandleProvider windowHandleProvider,
    ILogger<CameraProvider> logger) : ICameraProvider
{
    public async Task<CapturedPhoto?> CapturePhotoAsync()
    {
        try
        {
            var capture = new CameraCaptureUI();
            InitializeWithWindow.Initialize(capture, windowHandleProvider.WindowHandle);

            capture.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            capture.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.Large3M;

            var photo = await capture.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (photo is null) return null;

            var props = await photo.GetBasicPropertiesAsync();
            return new CapturedPhoto(photo.Path, (long)props.Size);
        }
        catch (Exception ex)
        {
            LogCameraCaptureFailed(ex);
            return null;
        }
    }

    [LoggerMessage(EventId = 4000, Level = LogLevel.Warning, Message = "Camera capture failed or was cancelled")]
    partial void LogCameraCaptureFailed(Exception ex);
}
