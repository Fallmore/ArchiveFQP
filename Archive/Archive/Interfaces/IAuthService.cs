using Archive.Models.Auth;

namespace Archive.Interfaces
{
	public interface IAuthService
	{
		Task<LoginResult> Login(LoginModel model);
		//Task<RegistrationResult> Register(RegistrationModel model);
		Task Logout();
		Task<string> GetAccessToken();
		Task<UserInfo> GetCurrentUser();
		Task<bool> RefreshToken();
	}
}
