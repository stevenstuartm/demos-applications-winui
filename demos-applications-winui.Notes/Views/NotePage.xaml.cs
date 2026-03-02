using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Core.Navigation;
using demos_applications_winui.Notes.ViewModels;

namespace demos_applications_winui.Notes.Views;

public sealed partial class NotePage : Page, INavigable
{
    public NoteViewModel ViewModel { get; }

    public NotePage(NoteViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    public Task OnNavigatedToAsync(NavigationContext context) => ViewModel.OnNavigatedToAsync(context);
    public void OnNavigatedFrom() => ViewModel.OnNavigatedFrom();
}
