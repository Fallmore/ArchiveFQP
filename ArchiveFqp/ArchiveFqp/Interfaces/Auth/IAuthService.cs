using ArchiveFqp.Models.Auth;
using System.Security.Claims;

namespace ArchiveFqp.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterModel model);
        Task<AuthResult> LoginAsync(LoginModel model);
        Task<AuthResult> ChangePasswordAsync(ChangePasswordModel model);
        Task LogoutAsync();
    }
}
