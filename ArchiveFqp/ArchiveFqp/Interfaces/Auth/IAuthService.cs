using ArchiveFqp.Models.Auth;

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
