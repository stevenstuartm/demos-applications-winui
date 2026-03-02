using System.Text.Json.Serialization;

namespace demos_applications_winui.Notes.Models;

[JsonSerializable(typeof(Note))]
internal partial class NoteJsonContext : JsonSerializerContext;
