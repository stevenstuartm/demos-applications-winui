using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Core.Navigation;
using demos_applications_winui.Notes.Models;
using demos_applications_winui.Notes.ViewModels;

namespace demos_applications_winui.Notes.Views;

public sealed partial class AllNotesPage : Page, INavigable
{
    public AllNotesViewModel ViewModel { get; }

    public AllNotesPage(AllNotesViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    public Task OnNavigatedToAsync(NavigationContext context) => ViewModel.OnNavigatedToAsync(context);
    public void OnNavigatedFrom() => ViewModel.OnNavigatedFrom();

    private void ItemsView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is Note note)
        {
            ViewModel.OpenNoteCommand.Execute(note);
        }
    }
}
