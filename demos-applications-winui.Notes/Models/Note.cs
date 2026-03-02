using System;
using System.Collections.Generic;

namespace demos_applications_winui.Notes.Models;

/// <summary>
/// Persistence model serialized as <c>note.json</c> inside each per-note folder.
/// </summary>
public class Note
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? HtmlContent { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public List<NoteAttachment>? Attachments { get; set; }
}
