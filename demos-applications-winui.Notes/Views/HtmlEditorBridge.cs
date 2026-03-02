using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using demos_applications_winui.Notes.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace demos_applications_winui.Notes.Views;

public class HtmlEditorBridge : IHtmlEditorBridge
{
    private readonly WebView2 _webView;
    private TaskCompletionSource? _readyTcs;

    public event EventHandler<string>? ContentChanged;

    public HtmlEditorBridge(WebView2 webView)
    {
        _webView = webView;
    }

    public async Task InitializeAsync()
    {
        _readyTcs = new TaskCompletionSource();

        await _webView.EnsureCoreWebView2Async();

        _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

        var assetsPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Assets");

        _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "app.local", assetsPath, CoreWebView2HostResourceAccessKind.Allow);

        _webView.CoreWebView2.Navigate("https://app.local/quill-editor.html");

        await _readyTcs.Task;
    }

    public async Task SetContentAsync(string html)
    {
        var escaped = JsonSerializer.Serialize(html);
        await _webView.CoreWebView2.ExecuteScriptAsync($"setContent({escaped})");
    }

    public async Task<string> GetContentAsync()
    {
        var result = await _webView.CoreWebView2.ExecuteScriptAsync("getContent()");
        return JsonSerializer.Deserialize<string>(result) ?? string.Empty;
    }

    public async Task SetReadOnlyAsync(bool readOnly)
    {
        var value = readOnly ? "true" : "false";
        await _webView.CoreWebView2.ExecuteScriptAsync($"setReadOnly({value})");
    }

    public async Task InsertImageAsync(string imageUrl)
    {
        var escaped = JsonSerializer.Serialize(imageUrl);
        await _webView.CoreWebView2.ExecuteScriptAsync($"insertImage({escaped})");
    }

    private void OnWebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        var message = args.TryGetWebMessageAsString();
        if (message is null) return;

        using var doc = JsonDocument.Parse(message);
        var type = doc.RootElement.GetProperty("type").GetString();

        switch (type)
        {
            case "ready":
                _readyTcs?.TrySetResult();
                break;

            case "contentChanged":
                var html = doc.RootElement.GetProperty("html").GetString() ?? string.Empty;
                ContentChanged?.Invoke(this, html);
                break;
        }
    }
}
