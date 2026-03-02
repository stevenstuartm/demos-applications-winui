using System;

namespace demos_applications_winui.Notes.Models;

/// <summary>
/// Metadata for a file attached to a note. The file itself lives at
/// <c>notes/{noteId}/attachments/{RelativePath}</c>.
/// </summary>
public class NoteAttachment
{
    public string? Id { get; set; }
    public string? FileName { get; set; }
    public string? RelativePath { get; set; }
    public NoteAttachmentType AttachmentType { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime AddedDate { get; set; }
}
