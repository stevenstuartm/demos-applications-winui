using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using demos_applications_winui.Notes.Models;

namespace demos_applications_winui.Notes.Services;

/// <summary>
/// Low-level storage operations for notes. Reads/writes per-note folders under
/// <c>ApplicationData.Current.LocalFolder/notes/{noteId}/</c>.
/// </summary>
public interface INotesClient
{
    Task<IReadOnlyList<Note>> GetAllNotesAsync();
    Task SaveNoteAsync(Note note);
    Task DeleteNoteAsync(string noteId);
    Task<string> SaveAttachmentAsync(string noteId, string fileName, Stream content);
    Task DeleteAttachmentAsync(string noteId, string relativePath);
    string GetAttachmentAbsolutePath(string noteId, string relativePath);
}
