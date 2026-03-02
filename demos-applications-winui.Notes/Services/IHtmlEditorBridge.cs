using System;
using System.Threading.Tasks;

namespace demos_applications_winui.Notes.Services;

public interface IHtmlEditorBridge
{
    Task InitializeAsync();
    Task SetContentAsync(string html);
    Task<string> GetContentAsync();
    Task SetReadOnlyAsync(bool readOnly);
    Task InsertImageAsync(string imageUrl);
    event EventHandler<string>? ContentChanged;
}
