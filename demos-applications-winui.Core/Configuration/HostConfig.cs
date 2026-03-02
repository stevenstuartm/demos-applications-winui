using CommunityToolkit.Mvvm.ComponentModel;

namespace demos_applications_winui.Core.Configuration;

public partial class HostConfig : ObservableObject, IHostConfig
{
    [ObservableProperty]
    public partial AppStage Stage { get; internal set; }

    [ObservableProperty]
    public partial AppEnvironment Environment { get; internal set; }
}
