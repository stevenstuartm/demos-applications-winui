using System.Threading.Tasks;
using demos_applications_winui.Auth.Models;

namespace demos_applications_winui.Auth.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string username, string password);
    void Logout();
}
