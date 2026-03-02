namespace demos_applications_winui.Core.Configuration;

public class HostConfig : IHostConfig
{
    public AppStage Stage { get; internal set; }
    public AppEnvironment Environment { get; internal set; }
}
