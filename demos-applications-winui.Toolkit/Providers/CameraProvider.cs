using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Storage;
using demos_applications_winui.Toolkit.Platform;
using CapturedPhoto = demos_applications_winui.Toolkit.Platform.CapturedPhoto;

namespace demos_applications_winui.Toolkit.Providers;

/// <summary>
/// Captures a photo using <see cref="MediaCapture"/> with a live preview
/// rendered via <see cref="MediaFrameReader"/> onto an <see cref="Image"/> control
/// inside a <see cref="ContentDialog"/>. Requires <see cref="SetXamlRoot"/>
/// to be called after the root frame loads.
/// </summary>
public class CameraProvider : ICameraProvider
{
    private XamlRoot? _xamlRoot;

    public void SetXamlRoot(XamlRoot xamlRoot) => _xamlRoot = xamlRoot;

    public async Task<CapturedPhoto?> CapturePhotoAsync()
    {
        if (_xamlRoot is null)
            throw new InvalidOperationException(
                "XamlRoot has not been set. Call SetXamlRoot() after the root frame is loaded.");

        var sourceGroups = await MediaFrameSourceGroup.FindAllAsync();
        var sourceGroup = sourceGroups.FirstOrDefault(g =>
            g.SourceInfos.Any(si => si.SourceKind == MediaFrameSourceKind.Color))
            ?? throw new InvalidOperationException("No camera device found.");

        var mediaCapture = new MediaCapture();
        await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
        {
            SourceGroup = sourceGroup,
            MemoryPreference = MediaCaptureMemoryPreference.Cpu
        });

        MediaFrameReader? reader = null;
        try
        {
            var colorInfo = sourceGroup.SourceInfos.First(
                si => si.SourceKind == MediaFrameSourceKind.Color);
            var frameSource = mediaCapture.FrameSources[colorInfo.Id];
            reader = await mediaCapture.CreateFrameReaderAsync(
                frameSource, MediaEncodingSubtypes.Bgra8);

            var previewImage = new Image { Width = 640, Height = 480 };
            var bitmapSource = new SoftwareBitmapSource();
            previewImage.Source = bitmapSource;

            SoftwareBitmap? latestFrame = null;
            var frameLock = new Lock();

            reader.FrameArrived += (sender, _) =>
            {
                using var frame = sender.TryAcquireLatestFrame();
                var bitmap = frame?.VideoMediaFrame?.SoftwareBitmap;
                if (bitmap is null) return;

                var forPreview = SoftwareBitmap.Convert(
                    bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                var forCapture = SoftwareBitmap.Convert(
                    bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                lock (frameLock)
                {
                    latestFrame?.Dispose();
                    latestFrame = forCapture;
                }

                previewImage.DispatcherQueue.TryEnqueue(async () =>
                {
                    try { await bitmapSource.SetBitmapAsync(forPreview); }
                    finally { forPreview.Dispose(); }
                });
            };

            await reader.StartAsync();

            CapturedPhoto? result = null;

            var dialog = new ContentDialog
            {
                Title = "Capture Photo",
                Content = previewImage,
                PrimaryButtonText = "Capture",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = _xamlRoot
            };

            dialog.PrimaryButtonClick += async (_, args) =>
            {
                var deferral = args.GetDeferral();
                try
                {
                    SoftwareBitmap? frameToSave;
                    lock (frameLock)
                    {
                        frameToSave = latestFrame;
                        latestFrame = null;
                    }

                    if (frameToSave is null) return;

                    var fileName = $"capture_{Guid.NewGuid():N}.jpg";
                    var tempFolder = await StorageFolder.GetFolderFromPathAsync(
                        Path.GetTempPath());
                    var file = await tempFolder.CreateFileAsync(
                        fileName, CreationCollisionOption.GenerateUniqueName);

                    using (frameToSave)
                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var encoder = await BitmapEncoder.CreateAsync(
                            BitmapEncoder.JpegEncoderId, stream);
                        encoder.SetSoftwareBitmap(frameToSave);
                        await encoder.FlushAsync();
                    }

                    var props = await file.GetBasicPropertiesAsync();
                    result = new CapturedPhoto(file.Path, (long)props.Size);
                }
                finally
                {
                    deferral.Complete();
                }
            };

            var dialogResult = await dialog.ShowAsync();

            lock (frameLock)
            {
                latestFrame?.Dispose();
                latestFrame = null;
            }

            return dialogResult == ContentDialogResult.Primary ? result : null;
        }
        finally
        {
            if (reader is not null)
            {
                await reader.StopAsync();
                reader.Dispose();
            }

            mediaCapture.Dispose();
        }
    }
}
