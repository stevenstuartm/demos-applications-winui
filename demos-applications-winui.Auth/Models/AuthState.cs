using CommunityToolkit.Mvvm.ComponentModel;
using demos_applications_winui.Core.Auth;

namespace demos_applications_winui.Auth.Models;

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
