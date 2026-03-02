namespace demos_applications_winui.Core.Configuration;

/// <summary>
/// Mutable implementation of <see cref="IHostConfig"/>. Populated once at startup
/// by <see cref="HostConfigResolver"/> and registered as a singleton.
/// Properties use <c>internal set</c> to prevent modification outside the Core assembly.
/// </summary>
public class HostConfig : IHostConfig
{
    public AppStage Stage { get; internal set; }
    public AppEnvironment Environment { get; internal set; }
}
