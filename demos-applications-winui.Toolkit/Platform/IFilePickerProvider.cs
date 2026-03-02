using System.Collections.Generic;
using System.Threading.Tasks;

namespace demos_applications_winui.Toolkit.Platform;

/// <summary>
/// Wraps <see cref="Windows.Storage.Pickers.FileOpenPicker"/> with HWND initialization.
/// Returns platform-independent <see cref="PickedFile"/> records.
/// </summary>
public interface IFilePickerProvider
{
    Task<IReadOnlyList<PickedFile>> PickFilesAsync(IReadOnlyList<string> fileTypeFilters);
}
