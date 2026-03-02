using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using demos_applications_winui.Core.Platform;

namespace demos_applications_winui.Toolkit.Providers;

public class FilePickerProvider(IWindowHandleProvider windowHandleProvider) : IFilePickerProvider
{
    public async Task<IReadOnlyList<PickedFile>> PickFilesAsync(IReadOnlyList<string> fileTypeFilters)
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, windowHandleProvider.WindowHandle);

        foreach (var filter in fileTypeFilters)
            picker.FileTypeFilter.Add(filter);

        var files = await picker.PickMultipleFilesAsync();
        var result = new List<PickedFile>();

        foreach (var file in files)
        {
            var props = await file.GetBasicPropertiesAsync();
            result.Add(new PickedFile(file.Name, file.Path, (long)props.Size));
        }

        return result;
    }
}
