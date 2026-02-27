using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using demos_applications_winui.Models;
using demos_applications_winui.Services;
using demos_applications_winui.Views;

namespace demos_applications_winui.ViewModels;

public partial class AllNotesViewModel(
    INotesService notesService,
    INavigationService navigationService) : ObservableObject
{
    public ObservableCollection<Note> Notes { get; } = [];

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
