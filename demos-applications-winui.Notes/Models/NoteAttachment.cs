using System;

namespace demos_applications_winui.Notes.Models;

public class NoteAttachment
{
    public string? Id { get; set; }
    public string? FileName { get; set; }
    public string? RelativePath { get; set; }
    public NoteAttachmentType AttachmentType { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime AddedDate { get; set; }
}

public enum NoteAttachmentType
{
    Image,
    Document,
    Other
}
