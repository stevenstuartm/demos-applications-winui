using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Toolkit.Navigation;
using demos_applications_winui.Notes.ViewModels;

namespace demos_applications_winui.Notes.Views;

public sealed partial class NotePage : Page, INavigable, IDisposable
{
    public NoteViewModel ViewModel { get; }

    public NotePage(NoteViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var bridge = new HtmlEditorBridge(EditorWebView);
        await bridge.InitializeAsync();
        ViewModel.SetEditorBridge(bridge);
        ViewModel.IsEditorReady = true;
    }

    public Task InitializeAsync(object? parameter) => ViewModel.InitializeAsync(parameter);

    public Task<bool> CanNavigateFromAsync() => ViewModel.CanNavigateFromAsync();

    public void Dispose()
    {
        ViewModel.Dispose();
    }
}
