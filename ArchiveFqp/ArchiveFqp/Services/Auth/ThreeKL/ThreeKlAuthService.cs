using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.User;
using ArchiveFqp.Models.Auth.ThreeKL;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.User;
using ArchiveFqp.Models.Settings.SettingsArchive;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace ArchiveFqp.Services.Auth.ThreeKL
{
    public class ThreeKlAuthService 
    {
        private readonly ILogger<ThreeKlAuthService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IReferenceDataService _refDataService;
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IUserService _userService;
        private readonly SettingsArchive _settings;
        private readonly static string s_prefix = "3kl_";

        public ThreeKlAuthService(ILogger<ThreeKlAuthService> logger,
            HttpClient httpClient, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor, IReferenceDataService refDataService,
            IDbContextFactory<ArchiveFqpContext> dbFactory, IUserService userService,
            SettingsArchive settings)
        {
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _refDataService = refDataService;
            _dbFactory = dbFactory;
            _userService = userService;
            _settings = settings;
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

        // Вход или регистрация пользователя 
        public async Task<bool> SignInWithThreeKlAsync(ThreeKlUserInfo userInfo)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();
            // Ищем существующую привязку аккаунта
            АккаунтПользователя? existingAccount = await context.АккаунтПользователяs
                .FirstOrDefaultAsync(a => a.Логин == s_prefix + userInfo.Id);

            if (existingAccount != null)
            {
                // Пользователь уже привязан - выполняем вход
                await SignInUserAsync(existingAccount, userInfo);
                return true;
            }

            return false;
        }

        public async Task<bool> LinkOrCreateAccount(ThreeKlUserInfo userInfo)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            List<Пользователь> existignUsers = await context.Пользовательs
                .Include(x => x.АккаунтПользователя)
                .Where(a => a.Фамилия == userInfo.Lastname
                                    && a.Имя == userInfo.Firstname
                                    && (userInfo.RegisterModel.Patronymic == null || userInfo.RegisterModel.Patronymic == a.Отчество)
                                    && a.АккаунтПользователя == null)
                .ToListAsync();

            АккаунтПользователя account3kl = new();

            if (existignUsers.Count == 0)
            {
                account3kl = await CreateUserAsync(userInfo);
            }
            else
            {
                account3kl = await LinkUserAsync(userInfo, existignUsers);
            }

            context.АккаунтПользователяs.Add(account3kl);
            await context.SaveChangesAsync();

            return true;
        }

        private async Task SignInUserAsync(АккаунтПользователя account, ThreeKlUserInfo userInfo)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            List<Claim> claims = [
                new Claim(ClaimTypes.NameIdentifier, account.IdПользователя.ToString()),
                new Claim(ClaimTypes.Name, $"{userInfo.Lastname} {userInfo.Firstname}"),
                ];

            // Добавляем роли в claims
            List<string> roleNames = await _userService.GetUserRoleNames(account);
            foreach (string role in roleNames)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new(identity);

            await _httpContextAccessor.HttpContext!.SignInAsync(principal);
        }

        private async Task<АккаунтПользователя> CreateUserAsync(ThreeKlUserInfo userInfo)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            // Создаем нового пользователя
            var newUser = new Пользователь
            {
                Фамилия = userInfo.Lastname,
                Имя = userInfo.Firstname,
                Отчество = userInfo.RegisterModel.Patronymic,
                Email = userInfo.Email
            };

            context.Пользовательs.Add(newUser);
            await context.SaveChangesAsync();

            List<РольПользователя> roles = await context.РольПользователяs.ToListAsync();
            List<int> userRoles = [];
            if (userInfo.RegisterModel.UserStructure.UserType == UserType.Student)
                userRoles.Add(roles.First(x => x.Название == _settings.RoleStudentName).IdРоли);
            else if (userInfo.RegisterModel.UserStructure.UserType == UserType.Teacher)
                userRoles.Add(roles.First(x => x.Название == _settings.RoleTeacherName).IdРоли);

            var account3kl = new АккаунтПользователя
            {
                IdПользователя = newUser.IdПользователя,
                Логин = s_prefix + userInfo.Id,
                Пароль = "",
                Роли = userRoles
            };

            if (userInfo.RegisterModel.UserStructure.UserType == UserType.Student)
            {
                Студент student = new()
                {
                    IdПользователя = newUser.IdПользователя,
                    IdИнститута = userInfo.RegisterModel.UserStructure.IdInstitute!.Value,
                    IdКафедры = userInfo.RegisterModel.UserStructure.IdDepartment!.Value,
                    IdНаправления = userInfo.RegisterModel.UserStructure.IdDirection!.Value,
                    IdПрофиля = userInfo.RegisterModel.UserStructure.IdProfile,
                    IdУровняОбразования = userInfo.RegisterModel.UserStructure.IdEducationLevel!.Value,
                    IdФормыОбучения = userInfo.RegisterModel.UserStructure.IdEducationForm!.Value,
                    ГодОкончания = userInfo.RegisterModel.UserStructure.YearGraduation!.Value,
                    Роли = [],
                    Активно = true
                };

                context.Студентs.Add(student);
                await context.SaveChangesAsync();
            }
            else if (userInfo.RegisterModel.UserStructure.UserType == UserType.Teacher)
            {
                Преподаватель teacher = new()
                {
                    IdПользователя = newUser.IdПользователя,
                    IdИнститута = userInfo.RegisterModel.UserStructure.IdInstitute!.Value,
                    IdКафедры = userInfo.RegisterModel.UserStructure.IdDepartment!.Value,
                    IdДолжности = userInfo.RegisterModel.UserStructure.IdPost!.Value,
                    Роли = [],
                    Активно = true
                };

                context.Преподавательs.Add(teacher);
                await context.SaveChangesAsync();
            }

            return account3kl;
        }

        private async Task<АккаунтПользователя> LinkUserAsync(ThreeKlUserInfo userInfo, List<Пользователь> candidates)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            АккаунтПользователя account3kl = new()
            {
                Логин = s_prefix + userInfo.Id,
                Пароль = "",
                Роли = []
            };
            List<РольПользователя> roles = await context.РольПользователяs.ToListAsync();

            if (userInfo.RegisterModel.UserStructure.UserType == UserType.Student)
            {
                List<Студент> foundStudents = await context.Студентs
                    .Where(x => x.IdИнститута == userInfo.RegisterModel.UserStructure.IdInstitute!.Value
                                    && x.IdКафедры == userInfo.RegisterModel.UserStructure.IdDepartment!.Value
                                    && x.IdНаправления == userInfo.RegisterModel.UserStructure.IdDirection!.Value
                                    && x.IdПрофиля == userInfo.RegisterModel.UserStructure.IdProfile
                                    && x.IdУровняОбразования == userInfo.RegisterModel.UserStructure.IdEducationLevel!.Value
                                    && x.IdФормыОбучения == userInfo.RegisterModel.UserStructure.IdEducationForm!.Value
                                    && x.ГодОкончания == userInfo.RegisterModel.UserStructure.YearGraduation!.Value
                                    && x.Активно == true)
                    .ToAsyncEnumerable()
                    .Where(x => candidates.Exists(y => y.IdПользователя == x.IdПользователя))
                    .ToListAsync();

                if (foundStudents.Count > 2)
                {
                    _logger.LogCritical(
                        "Найдено 2 студента одинаковой структуры без аккаунтов с ФИО {LastName} {Firstname} {Patromymic}",
                        userInfo.Lastname, userInfo.Firstname, userInfo.RegisterModel.Patronymic ?? "");
                }

                if (foundStudents.Count != 0)
                {
                    account3kl.IdПользователя = foundStudents[0].IdПользователя;
                    account3kl.Роли = [roles.First(x => x.Название == _settings.RoleStudentName).IdРоли];
                }
            }
            else if (userInfo.RegisterModel.UserStructure.UserType == UserType.Teacher)
            {
                List<Преподаватель> foundTeachers = await context.Преподавательs
                    .Where(x => x.IdИнститута == userInfo.RegisterModel.UserStructure.IdInstitute!.Value
                                    && x.IdКафедры == userInfo.RegisterModel.UserStructure.IdDepartment!.Value
                                    && x.Активно == true)
                    .ToAsyncEnumerable()
                    .Where(x => candidates.Exists(y => y.IdПользователя == x.IdПользователя))
                    .ToListAsync();

                if (foundTeachers.Count > 2)
                {
                    _logger.LogCritical(
                        "Найдено 2 преподавателя одинаковой структуры без аккаунтов с ФИО {LastName} {Firstname} {Patromymic}",
                        userInfo.Lastname, userInfo.Firstname, userInfo.RegisterModel.Patronymic ?? "");
                }

                if (foundTeachers.Count != 0)
                {
                    account3kl.IdПользователя = foundTeachers[0].IdПользователя;
                    account3kl.Роли = [roles.First(x => x.Название == _settings.RoleTeacherName).IdРоли];
                }
            }

            if (account3kl.IdПользователя == 0 || account3kl.Роли.Count == 0)
            {
                account3kl = await CreateUserAsync(userInfo);
            }

            return account3kl;
        }
    }
}
