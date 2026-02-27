using demos_applications_winui.Modals;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace demos_applications_winui.Views;

public sealed partial class NotePage : Page
{
    private Note? noteModel = null;

    public NotePage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Note note)
        {
            noteModel = note;
        }
        else
        {
            noteModel = new Note();
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (noteModel is not null)
        {
            noteModel.Text = NoteEditor.Text;
            
            await noteModel.SaveAsync();

            if (Frame.CanGoBack == true)
            {
                Frame.GoBack();
            }
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (noteModel is not null)
        {
            await noteModel.DeleteAsync();
        }

        if (Frame.CanGoBack == true)
        {
            Frame.GoBack();
        }
    }
}
