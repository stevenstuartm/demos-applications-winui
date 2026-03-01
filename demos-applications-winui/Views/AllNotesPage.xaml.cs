using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Models;
using demos_applications_winui.Services;
using demos_applications_winui.ViewModels;

namespace demos_applications_winui.Views;

public sealed partial class AllNotesPage(AllNotesViewModel viewModel) : Page, INavigable
{
    public AllNotesViewModel ViewModel { get; } = viewModel;

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
