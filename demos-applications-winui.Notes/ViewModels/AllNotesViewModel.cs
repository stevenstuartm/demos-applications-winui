using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using demos_applications_winui.Core.Navigation;
using demos_applications_winui.Notes.Models;
using demos_applications_winui.Notes.Services;
using demos_applications_winui.Notes.Views;

namespace demos_applications_winui.Notes.ViewModels;

public partial class AllNotesViewModel(
    INotesService notesService,
    INavigationService navigationService) : ObservableObject, INavigable
{
    public ObservableCollection<Note> Notes { get; } = [];

    public async Task OnNavigatedToAsync(NavigationContext context)
    {
        await LoadNotesCommand.ExecuteAsync(null);
    }

    public void OnNavigatedFrom()
    {
        LoadNotesCommand.Cancel();
    }

    [RelayCommand]
    private async Task LoadNotesAsync()
    {
        Notes.Clear();

        var notes = await notesService.LoadNotesAsync();

        foreach (var note in notes)
        {
            Notes.Add(note);
        }
    }

    [RelayCommand]
    private void AddNote()
    {
        navigationService.NavigateTo<NotePage>(new Note());
    }

    [RelayCommand]
    private void OpenNote(Note note)
    {
        navigationService.NavigateTo<NotePage>(note);
    }
}
