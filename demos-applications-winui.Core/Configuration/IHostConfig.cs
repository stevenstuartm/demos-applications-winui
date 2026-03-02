namespace demos_applications_winui.Core.Configuration;

/// <summary>
/// Read-only host configuration: deployment stage and target environment.
/// Inject this in any service that needs to branch behavior based on deployment context.
/// </summary>
public interface IHostConfig
{
    AppStage Stage { get; }
    AppEnvironment Environment { get; }
}
