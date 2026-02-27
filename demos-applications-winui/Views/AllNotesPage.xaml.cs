using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using demos_applications_winui.Models;
using demos_applications_winui.ViewModels;

namespace demos_applications_winui.Views;

public sealed partial class AllNotesPage : Page
{
    public AllNotesViewModel ViewModel { get; }

    public AllNotesPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<AllNotesViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        await ViewModel.LoadNotesCommand.ExecuteAsync(null);
    }

    private void ItemsView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is Note note)
        {
            ViewModel.OpenNoteCommand.Execute(note);
        }
    }
}
