using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using demos_applications_winui.Notes.Models;

namespace demos_applications_winui.Notes.Services;

public class NotesService(INotesClient notesClient) : INotesService
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg"
    };

    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".pptx", ".txt", ".csv"
    };

    public Task<IReadOnlyList<Note>> LoadNotesAsync() =>
        notesClient.GetAllNotesAsync();

    public Task SaveNoteAsync(Note note) =>
        notesClient.SaveNoteAsync(note);

    public Task DeleteNoteAsync(string noteId) =>
        notesClient.DeleteNoteAsync(noteId);

    public async Task<NoteAttachment> AddAttachmentAsync(string noteId, string fileName, Stream content)
    {
        var relativePath = await notesClient.SaveAttachmentAsync(noteId, fileName, content);

        var extension = Path.GetExtension(fileName);
        var attachmentType = ImageExtensions.Contains(extension)
            ? NoteAttachmentType.Image
            : DocumentExtensions.Contains(extension)
                ? NoteAttachmentType.Document
                : NoteAttachmentType.Other;

        return new NoteAttachment
        {
            Id = Guid.NewGuid().ToString("N"),
            FileName = fileName,
            RelativePath = relativePath,
            AttachmentType = attachmentType,
            FileSizeBytes = content.Length,
            AddedDate = DateTime.Now
        };
    }

    public Task RemoveAttachmentAsync(string noteId, string attachmentId, string relativePath) =>
        notesClient.DeleteAttachmentAsync(noteId, relativePath);

    public string GetAttachmentAbsolutePath(string noteId, string relativePath) =>
        notesClient.GetAttachmentAbsolutePath(noteId, relativePath);
}
