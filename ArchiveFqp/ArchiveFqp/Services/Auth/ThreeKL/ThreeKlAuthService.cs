using ArchiveFqp.Models.Auth.ThreeKL;
using ArchiveFqp.Models.Database;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ArchiveFqp.Services.Auth.ThreeKL
{
    public class ThreeKlAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;

        public ThreeKlAuthService(
            HttpClient httpClient,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IDbContextFactory<ArchiveFqpContext> dbFactory)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _dbFactory = dbFactory;
        }

        // Генерация URL для перенаправления на страницу авторизации 3KL
        public string GetAuthorizationUrl()
        {
            string clientId = _configuration["ThreeKl:ClientId"]!;
            string redirectUri = _configuration["ThreeKl:RedirectUri"]!;
            string authEndpoint = _configuration["ThreeKl:AuthorizationEndpoint"]!;
            string state = Guid.NewGuid().ToString("N");

            // Сохраняем state в cookie для проверки при возврате
            var httpContext = _httpContextAccessor.HttpContext;
            httpContext?.Response.Cookies.Append("OAuthState", state, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(10)
            });

            return $"{authEndpoint}?" +
               $"client_id={Uri.EscapeDataString(clientId)}&" +
               $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
               $"response_type=code&" +
               $"scope=user_info&" +
               $"state={state}";
        }

        // Обмен авторизационного кода на токен доступа
        public async Task<ThreeKlTokens?> ExchangeCodeForTokensAsync(string code)
        {
            string tokenEndpoint = _configuration["ThreeKl:TokenEndpoint"]!;
            string clientId = _configuration["ThreeKl:ClientId"]!;
            string clientSecret = _configuration["ThreeKl:ClientSecret"]!;
            string redirectUri = _configuration["ThreeKl:RedirectUri"]!;

            FormUrlEncodedContent content = new(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri
            });

            HttpResponseMessage response = await _httpClient.PostAsync(tokenEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ThreeKlTokens>(json);
            }

            return null;
        }

        // Получение данных пользователя из 3KL
        public async Task<ThreeKlUserInfo?> GetUserInfoAsync(string accessToken)
        {
            var userInfoEndpoint = _configuration["ThreeKl:UserInfoEndpoint"];

            using var request = new HttpRequestMessage(HttpMethod.Get, userInfoEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ThreeKlUserInfo>(json);
            }

            return null;
        }

        // Вход или регистрация пользователя в вашей системе
        public async Task<bool> SignInWithThreeKlAsync(ThreeKlUserInfo userInfo)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();
            // Ищем существующую привязку аккаунта
            var existingAccount = await context.АккаунтПользователяs
                .Include(a => a.IdПользователяNavigation)
                .FirstOrDefaultAsync(a => a.Логин == $"3kl_{userInfo.Id}");

            if (existingAccount != null)
            {
                // Пользователь уже привязан - выполняем вход
                await SignInUserAsync(existingAccount, userInfo);
                return true;
            }

            // Ищем пользователя по email
            if (!string.IsNullOrEmpty(userInfo.Email))
            {
                var userByEmail = await context.Пользовательs
                    .FirstOrDefaultAsync(u => u.Email == userInfo.Email);

                if (userByEmail != null)
                {
                    // Привязываем существующий аккаунт к 3KL
                    АккаунтПользователя newAccount = new()
                    {
                        IdПользователя = userByEmail.IdПользователя,
                        Логин = $"3kl_{userInfo.Id}",
                        Пароль = "", // Пароль не нужен для OAuth
                        Роли = [1]
                    };

                    context.АккаунтПользователяs.Add(newAccount);
                    await context.SaveChangesAsync();

                    // Получаем аккаунт с пользователем для входа
                    var account = await context.АккаунтПользователяs
                        .Include(a => a.IdПользователяNavigation)
                        .FirstAsync(a => a.IdПользователя == userByEmail.IdПользователя);

                    await SignInUserAsync(account, userInfo);
                    return true;
                }
            }

            // Создаем нового пользователя
            var newUser = new Пользователь
            {
                Фамилия = userInfo.Lastname,
                Имя = userInfo.Firstname,
                Email = userInfo.Email
            };

            context.Пользовательs.Add(newUser);
            await context.SaveChangesAsync();

            var account3kl = new АккаунтПользователя
            {
                IdПользователя = newUser.IdПользователя,
                Логин = $"3kl_{userInfo.Id}",
                Пароль = "",
                Роли = [1]
            };

            context.АккаунтПользователяs.Add(account3kl);
            await context.SaveChangesAsync();

            var fullAccount = await context.АккаунтПользователяs
                .Include(a => a.IdПользователяNavigation)
                .FirstAsync(a => a.IdПользователя == newUser.IdПользователя);

            await SignInUserAsync(fullAccount, userInfo);
            return true;
        }

        private async Task SignInUserAsync(АккаунтПользователя account, ThreeKlUserInfo userInfo)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            var roles = await context.РольПользователяs
                .Where(r => account.Роли.Contains(r.IdРоли))
                .Select(r => r.Название)
                .ToListAsync();

            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.IdПользователя.ToString()),
            new(ClaimTypes.Name, $"{account.IdПользователяNavigation!.Фамилия} {account.IdПользователяNavigation.Имя}"),
            new("AuthProvider", "3kl")
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await _httpContextAccessor.HttpContext!.SignInAsync(principal);
        }
    }
}
