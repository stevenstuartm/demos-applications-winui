using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using demos_applications_winui.Notes.Models;

namespace demos_applications_winui.Notes.Services;

public interface INotesClient
{
    Task<IReadOnlyList<Note>> GetAllNotesAsync();
    Task SaveNoteAsync(Note note);
    Task DeleteNoteAsync(string noteId);
    Task<string> SaveAttachmentAsync(string noteId, string fileName, Stream content);
    Task DeleteAttachmentAsync(string noteId, string relativePath);
    string GetAttachmentAbsolutePath(string noteId, string relativePath);
}
