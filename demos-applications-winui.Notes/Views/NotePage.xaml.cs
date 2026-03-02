using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Toolkit.Navigation;
using demos_applications_winui.Notes.ViewModels;

namespace demos_applications_winui.Notes.Views;

public sealed partial class NotePage : Page, INavigable
{
    public NoteViewModel ViewModel { get; }
    private HtmlEditorBridge? _bridge;

    public NotePage(NoteViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Initializes the WebView2 editor bridge on first load. The null-check guards
    /// against re-initialization since this is a singleton page that may be Loaded
    /// multiple times across navigations.
    /// </summary>
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_bridge is not null) return;

        _bridge = new HtmlEditorBridge(EditorWebView);
        await _bridge.InitializeAsync();
        ViewModel.SetEditorBridge(_bridge);
        ViewModel.IsEditorReady = true;
    }

    public Task OnNavigatedToAsync(NavigationContext context) =>
        ViewModel.OnNavigatedToAsync(context);

    public void OnNavigatedFrom() => ViewModel.OnNavigatedFrom();
}
