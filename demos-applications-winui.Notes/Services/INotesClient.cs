using System.Collections.Generic;
using System.Threading.Tasks;
using demos_applications_winui.Notes.Models;

namespace demos_applications_winui.Notes.Services;

public interface INotesClient
{
    Task<IReadOnlyList<Note>> GetAllNotesAsync();
    Task SaveNoteAsync(Note note);
    Task DeleteNoteAsync(Note note);
}
