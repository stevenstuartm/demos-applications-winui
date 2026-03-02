using System;

namespace demos_applications_winui.Core.Configuration;

/// <summary>
/// Resolves logging configuration from environment variables with safe defaults.
/// </summary>
public static class LoggingConfigResolver
{
    public const string LogDirectoryEnvVar = "APP_LOG_DIRECTORY";

    public static LoggingConfig Resolve()
    {
        var config = new LoggingConfig();

        var logDir = Environment.GetEnvironmentVariable(LogDirectoryEnvVar);
        if (!string.IsNullOrEmpty(logDir))
            config.LogDirectory = logDir;

        return config;
    }
}
