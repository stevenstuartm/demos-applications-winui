namespace demos_applications_winui.Toolkit.Platform;

/// <summary>
/// Temporary file produced by a camera capture. The caller is responsible
/// for copying or moving the file before it is cleaned up.
/// </summary>
public record CapturedPhoto(string TempFilePath, long SizeBytes);
