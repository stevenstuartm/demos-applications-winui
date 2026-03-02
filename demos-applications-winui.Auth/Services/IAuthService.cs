using System.Threading.Tasks;
using demos_applications_winui.Auth.Models;

namespace demos_applications_winui.Auth.Services;

/// <summary>
/// Authentication actions. Login navigates to the default page on success;
/// Logout clears state and redirects to the login page.
/// </summary>
public interface IAuthService
{
    Task<AuthResult> LoginAsync(string username, string password);
    void Logout();
}
