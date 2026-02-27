using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using demos_applications_winui.Models;
using demos_applications_winui.ViewModels;

namespace demos_applications_winui.Views;

public sealed partial class NotePage : Page
{
    public NoteViewModel ViewModel { get; }

    public NotePage()
    {
        ViewModel = App.Current.Services.GetRequiredService<NoteViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Note note)
        {
            ViewModel.LoadNote(note);
        }
    }
}
