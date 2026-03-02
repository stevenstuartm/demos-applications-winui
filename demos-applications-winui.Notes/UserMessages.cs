namespace demos_applications_winui.Notes;

internal static class UserMessages
{
    internal static class Toast
    {
        internal const string SaveSuccessTitle = "Saved";
        internal const string SaveSuccessMessage = "Note saved successfully.";
        internal const string ValidationErrorTitle = "Error";
        internal const string ValidationErrorMessage = "Fix errors before saving.";
    }

    internal static class Dialog
    {
        internal const string UnsavedChangesTitle = "Unsaved changes";
        internal const string UnsavedChangesMessage = "You have unsaved changes. Discard and leave?";
        internal const string DeleteNoteTitle = "Delete note";
        internal const string DeleteNoteMessage = "Are you sure you want to delete this note? This cannot be undone.";
    }
}
