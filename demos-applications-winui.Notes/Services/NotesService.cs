using System.Collections.Generic;
using System.Threading.Tasks;
using demos_applications_winui.Notes.Models;

namespace demos_applications_winui.Notes.Services;

public class NotesService(INotesClient notesClient) : INotesService
{
    public Task<IReadOnlyList<Note>> LoadNotesAsync() =>
        notesClient.GetAllNotesAsync();

    public Task SaveNoteAsync(Note note) =>
        notesClient.SaveNoteAsync(note);

    public Task DeleteNoteAsync(Note note) =>
        notesClient.DeleteNoteAsync(note);
}
