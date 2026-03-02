namespace demos_applications_winui.Core.Configuration;

public class LoggingConfig
{
    /// <summary>
    /// Directory where log files are written.
    /// Defaults to ApplicationData.Current.LocalFolder.Path + "/logs" when null.
    /// </summary>
    public string? LogDirectory { get; set; }

    /// <summary>
    /// Rolling file name template (Serilog format).
    /// Produces files like app-20260302.log.
    /// </summary>
    public string? FileNameTemplate { get; set; }

    /// <summary>
    /// Max bytes per file before rollover. Null means consumer decides the default.
    /// </summary>
    public long? FileSizeLimitBytes { get; set; }

    /// <summary>
    /// Number of rolling files to retain. Null means consumer decides the default.
    /// </summary>
    public int? RetainedFileCountLimit { get; set; }
}
