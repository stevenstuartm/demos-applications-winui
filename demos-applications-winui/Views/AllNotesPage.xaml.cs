using demos_applications_winui.Modals;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace demos_applications_winui.Views;

public sealed partial class AllNotesPage : Page
{
    private AllNotes notesModel = new AllNotes();

    public AllNotesPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        notesModel.LoadNotes();
    }

    private void OnNewNoteClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame.Navigate(typeof(NotePage), new Note());
    }

    private void ItemsView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is Note note)
        {
            Frame.Navigate(typeof(NotePage), note);
        }
    }
}
