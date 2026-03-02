using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using demos_applications_winui.Core.Navigation;
using demos_applications_winui.Notes.Models;
using demos_applications_winui.Notes.Services;
using demos_applications_winui.Notes.Views;

namespace demos_applications_winui.Notes.ViewModels;

public partial class AllNotesViewModel(
    INotesService notesService,
    INavigationService navigationService,
    ILogger<AllNotesViewModel> logger) : ObservableObject, INavigable
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
        LogNotesLoaded(notes.Count);

        foreach (var note in notes)
        {
            Notes.Add(note);
        }
    }

    [RelayCommand]
    private async Task AddNoteAsync()
    {
        var now = System.DateTime.Now;
        var note = new Note
        {
            Id = System.Guid.NewGuid().ToString("N"),
            CreatedDate = now,
            ModifiedDate = now,
            Attachments = []
        };
        await navigationService.NavigateToAsync<NotePage>(note);
    }

    [RelayCommand]
    private async Task OpenNoteAsync(Note note)
    {
        await navigationService.NavigateToAsync<NotePage>(note);
    }

    [LoggerMessage(EventId = 6000, Level = LogLevel.Debug, Message = "Loaded {Count} notes")]
    partial void LogNotesLoaded(int count);
}
