using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using demos_applications_winui.Core.Navigation;
using demos_applications_winui.Notes.Models;
using demos_applications_winui.Notes.Services;

namespace demos_applications_winui.Notes.ViewModels;

public partial class NoteViewModel(
    INotesService notesService,
    INavigationService navigationService) : ObservableObject, INavigable
{
    [ObservableProperty]
    public partial string Text { get; set; }

    [ObservableProperty]
    public partial DateTime Date { get; set; }

    [ObservableProperty]
    public partial string Filename { get; set; }

    public Task OnNavigatedToAsync(NavigationContext context)
    {
        if (context.Parameter is Note note)
        {
            Text = note.Text;
            Date = note.Date;
            Filename = note.Filename;
        }

        return Task.CompletedTask;
    }

    public void OnNavigatedFrom()
    {
        SaveCommand.Cancel();
        DeleteCommand.Cancel();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var note = new Note
        {
            Filename = Filename,
            Text = Text,
            Date = Date
        };

        await notesService.SaveNoteAsync(note);
        navigationService.GoBack();
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        var note = new Note
        {
            Filename = Filename,
            Text = Text,
            Date = Date
        };

        await notesService.DeleteNoteAsync(note);
        navigationService.GoBack();
    }
}
