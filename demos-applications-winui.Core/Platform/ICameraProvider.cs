using System.Threading.Tasks;

namespace demos_applications_winui.Core.Platform;

public interface ICameraProvider
{
    Task<CapturedPhoto?> CapturePhotoAsync();
}

public record CapturedPhoto(string TempFilePath, long SizeBytes);
