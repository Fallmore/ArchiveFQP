namespace Archive.Models.Auth
{
	public class LoginModel
	{
		public string Email { get; set; } = "";
		public string Password { get; set; } = "";
		public bool RememberMe { get; set; }
	}

	public class RegistrationModel
	{
		public string Email { get; set; } = "";
		public string Password { get; set; } = "";
		public string ConfirmPassword { get; set; } = "";
		public string? Name { get; set; }
		public string? Role { get; set; }
	}

	public class LoginResponse
	{
		public string Token { get; set; } = "";
		public string RefreshToken { get; set; } = "";
		public UserDto User { get; set; } = new();
		public DateTime ExpiresAt { get; set; }
	}

	public class UserDto
	{
		public string Id { get; set; } = "";
		public string? Name { get; set; }
		public string? Email { get; set; }
		public string? Role { get; set; }
	}

	public class RefreshTokenResponse
	{
		public string Token { get; set; } = "";
		public string RefreshToken { get; set; } = "";
	}

	public class LoginResult
	{
		public bool Success { get; set; }
		public string? Token { get; set; }
		public string? RefreshToken { get; set; }
		public UserInfo? User { get; set; }
		public string? Error { get; set; }
	}

	public class UserInfo
	{
		public string Id { get; set; } = "";
		public string? Name { get; set; }
		public string? Email { get; set; }
		public string? Role { get; set; }
		public Dictionary<string, object> Claims { get; set; } = new();
	}
}
