using System.Collections.Generic;
using System.Threading.Tasks;

namespace demos_applications_winui.Toolkit.Platform;

public interface IFilePickerProvider
{
    Task<IReadOnlyList<PickedFile>> PickFilesAsync(IReadOnlyList<string> fileTypeFilters);
}

public record PickedFile(string FileName, string Path, long SizeBytes);
