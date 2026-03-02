using System.Text.Json.Serialization;

namespace demos_applications_winui.Notes.Models;

/// <summary>
/// Source-generated JSON serializer context for AOT-safe <see cref="Note"/> serialization.
/// </summary>
[JsonSerializable(typeof(Note))]
internal partial class NoteJsonContext : JsonSerializerContext;
