using System.ComponentModel;

namespace demos_applications_winui.Core.Auth;

public interface IAuthState : INotifyPropertyChanged
{
    bool IsAuthenticated { get; }
    string? Username { get; }
}
