using System;
using System.Threading.Tasks;

namespace demos_applications_winui.Notes.Services;

/// <summary>
/// Abstraction over the Quill.js WebView2 editor. The contract lives in Services
/// so the VM can depend on it; the implementation (<see cref="Views.HtmlEditorBridge"/>)
/// lives in Views because it wraps a UI control.
/// </summary>
public interface IHtmlEditorBridge
{
    Task InitializeAsync();
    Task SetContentAsync(string html);
    Task<string> GetContentAsync();
    Task SetReadOnlyAsync(bool readOnly);
    Task InsertImageAsync(string imageUrl);
    event EventHandler<string>? ContentChanged;
}
