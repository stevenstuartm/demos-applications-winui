using CommunityToolkit.Mvvm.ComponentModel;
using demos_applications_winui.Core.Auth;

namespace demos_applications_winui.Auth.Models;

/// <summary>
/// Mutable authentication state. <see cref="AuthService"/> calls <see cref="SetAuthenticated"/>
/// and <see cref="Clear"/>; all other consumers see the read-only <see cref="IAuthState"/> projection.
/// </summary>
public partial class AuthState : ObservableObject, IAuthState
{
    [ObservableProperty]
    public partial bool IsAuthenticated { get; set; }

    [ObservableProperty]
    public partial string? Username { get; set; }

    public void SetAuthenticated(string username)
    {
        Username = username;
        IsAuthenticated = true;
    }

    public void Clear()
    {
        Username = null;
        IsAuthenticated = false;
    }
}
