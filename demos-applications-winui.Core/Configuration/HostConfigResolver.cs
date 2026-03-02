using System;

namespace demos_applications_winui.Core.Configuration;

/// <summary>
/// Resolves host configuration from environment variables with safe defaults.
/// Pure function — no DI, no side effects.
/// </summary>
public static class HostConfigResolver
{
    public const string StageEnvVar = "APP_STAGE";
    public const string EnvironmentEnvVar = "APP_ENVIRONMENT";

    public static HostConfig Resolve()
    {
        return new HostConfig
        {
            Stage = ParseEnum(
                Environment.GetEnvironmentVariable(StageEnvVar),
                AppStage.Local),
            Environment = ParseEnum(
                Environment.GetEnvironmentVariable(EnvironmentEnvVar),
                AppEnvironment.Dev)
        };
    }

    private static T ParseEnum<T>(string? value, T defaultValue) where T : struct, Enum =>
        Enum.TryParse(value, ignoreCase: true, out T result) ? result : defaultValue;
}
