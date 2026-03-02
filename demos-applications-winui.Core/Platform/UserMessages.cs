namespace demos_applications_winui.Core.Platform;

public static class UserMessages
{
    public static class Toast
    {
        // Global error handling
        public const string UnexpectedErrorTitle = "Something went wrong";
        public const string UnexpectedErrorMessage = "An unexpected error occurred. Please try again.";
        public const string BackgroundErrorMessage = "A background error occurred.";

        // Notes
        public const string SaveSuccessTitle = "Saved";
        public const string SaveSuccessMessage = "Note saved successfully.";
        public const string ValidationErrorTitle = "Error";
        public const string ValidationErrorMessage = "Fix errors before saving.";
    }

    public static class Dialog
    {
        // Unsaved changes
        public const string UnsavedChangesTitle = "Unsaved changes";
        public const string UnsavedChangesMessage = "You have unsaved changes. Discard and leave?";

        // Delete confirmation
        public const string DeleteNoteTitle = "Delete note";
        public const string DeleteNoteMessage = "Are you sure you want to delete this note? This cannot be undone.";
    }
}
