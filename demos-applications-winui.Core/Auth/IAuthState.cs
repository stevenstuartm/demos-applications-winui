using System.ComponentModel;

namespace demos_applications_winui.Core.Auth;

/// <summary>
/// Read-only identity contract exposed to all layers. Domains bind to this to
/// react to auth changes without depending on the mutable Auth domain internals.
/// </summary>
public interface IAuthState : INotifyPropertyChanged
{
    bool IsAuthenticated { get; }
    string? Username { get; }
}
