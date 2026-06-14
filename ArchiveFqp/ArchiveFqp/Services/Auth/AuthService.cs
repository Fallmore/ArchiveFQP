using ArchiveFqp.Interfaces.Auth;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.User;
using ArchiveFqp.Models.Auth;
using ArchiveFqp.Models.Auth.ThreeKL;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.User;
using ArchiveFqp.Models.Settings.SettingsArchive;
using ArchiveFqp.Services.Auth.ThreeKL;
using ArchiveFqp.Services.Notifications;
using ArchiveFqp.Services.User;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ArchiveFqp.Services.Auth
{

    public class AuthService : IAuthService
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IUserService _userService;
        private readonly ILogger<AuthService> _logger;
        private readonly SettingsArchive _settings;
        private readonly IReferenceDataService _refDataService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IDbContextFactory<ArchiveFqpContext> dbFactory,
            IUserService userService, ILogger<AuthService> logger,
            SettingsArchive settings, IReferenceDataService refDataService,
            IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _dbFactory = dbFactory;
            _userService = userService;
            _logger = logger;
            _settings = settings;
            _refDataService = refDataService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        

        // TODO: добавить отправку уведомления на почту при регистрации
        // TODO: добавить обновление данных пользователя, находящегося в сессии (роли)
        // TODO: добавить выход пользователя, находящегося в сессии, из аккаунта при смене пароля
        public async Task<AuthResult> RegisterAsync(RegisterModel model)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            // Проверка существования логина
            АккаунтПользователя? existingAccount = await context.АккаунтПользователяs
                .FirstOrDefaultAsync(a => a.Логин == model.Login);

            if (existingAccount != null)
            {
                return new AuthResult { Success = false, Message = "Пользователь с таким логином уже существует" };
            }

            if (!string.IsNullOrEmpty(model.Email))
            {
                // Проверка существования почты
                Пользователь? existingUser = await context.Пользовательs
                    .FirstOrDefaultAsync(a => a.Email == model.Email);

                if (existingUser != null)
                {
                    return new AuthResult { Success = false, Message = "Пользователь с такой почтой уже существует" };
                }
            }

            List<Пользователь> existignUsers = await context.Пользовательs
                .Include(x => x.АккаунтПользователя)
                .Where(a => a.Фамилия == model.Surname
                                    && a.Имя == model.Name
                                    && (model.Patronymic == null || model.Patronymic == a.Отчество)
                                    && a.АккаунтПользователя == null)
                .ToListAsync();

            АккаунтПользователя account3kl = new();

            if (existignUsers.Count == 0)
            {
                account3kl = await CreateUserAsync(model);
            }
            else
            {
                account3kl = await LinkUserAsync(model, existignUsers);
            }

            context.АккаунтПользователяs.Add(account3kl);
            await context.SaveChangesAsync();

            return new AuthResult
            {
                Success = true,
                Message = "Регистрация успешна"
            };
        }

        public async Task<AuthResult> LoginAsync(LoginModel model)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            АккаунтПользователя? account = await context.АккаунтПользователяs
                .Include(a => a.IdПользователяNavigation)
                .FirstOrDefaultAsync(a => a.Логин == model.Login);

            if (account == null || string.IsNullOrWhiteSpace(model.Password) || !BCrypt.Net.BCrypt.Verify(model.Password, account.Пароль))
            {
                return new AuthResult { Success = false, Message = "Неверный логин или пароль" };
            }

            List<Claim> claims = [
                new Claim(ClaimTypes.NameIdentifier, account.IdПользователя.ToString()),
                new Claim(ClaimTypes.Name, $"{account.IdПользователяNavigation.Фамилия} {account.IdПользователяNavigation.Имя}"),
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

            return new AuthResult
            {
                Success = true,
                Message = "Вход выполнен успешно",
                //Token = token,
                //User = userSession
            };
        }

        public async Task<AuthResult> ChangePasswordAsync(ChangePasswordModel model)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            АккаунтПользователя? account = await context.АккаунтПользователяs
                .FirstOrDefaultAsync(a => a.Логин == model.Login);

            if (account == null)
            {
                return new AuthResult { Success = false, Message = "Аккаунт не найден" };
            }

            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, account.Пароль))
            {
                return new AuthResult { Success = false, Message = "Пароль должен совпадать с текущим" };
            }

            account.Пароль = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await context.SaveChangesAsync();

            return new AuthResult
            {
                Success = true,
                Message = "Вход выполнен успешно",
                //Token = token,
                //User = userSession
            };
        }

        public async Task<bool> LogoutAsync()
        {
            if (_httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false)
            {
                await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return true;
            }

            return false;
        }

        private string GenerateJwtToken(UserSession user)
        {
            IConfigurationSection jwtSettings = _configuration.GetSection("JwtSettings");
            byte[] secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

            List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.ФИО),
            new Claim("login", user.Логин)
        ];

            // Добавляем роли в claims
            foreach (string role in user.Роли)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationInMinutes"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(secretKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler tokenHandler = new();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<АккаунтПользователя> CreateUserAsync(RegisterModel model)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            // Создаем нового пользователя
            var newUser = new Пользователь
            {
                Фамилия = model.Surname!,
                Имя = model.Name!,
                Отчество = model.Patronymic,
                Email = model.Email
            };

            context.Пользовательs.Add(newUser);
            await context.SaveChangesAsync();

            List<РольУчреждения> rolesOrganization = await _refDataService.GetAsync<РольУчреждения>();
            if (model.UserStructure.UserType == UserType.Student)
            {
                Студент student = new()
                {
                    IdПользователя = newUser.IdПользователя,
                    IdИнститута = model.UserStructure.IdInstitute!.Value,
                    IdКафедры = model.UserStructure.IdDepartment!.Value,
                    IdНаправления = model.UserStructure.IdDirection!.Value,
                    IdПрофиля = model.UserStructure.IdProfile,
                    IdУровняОбразования = model.UserStructure.IdEducationLevel!.Value,
                    IdФормыОбучения = model.UserStructure.IdEducationForm!.Value,
                    ГодОкончания = model.UserStructure.YearGraduation!.Value,
                    Роли = [rolesOrganization.First(x => x.Название == _settings.RoleStudentOnVerifyName).IdРоли],
                    Активно = true
                };

                context.Студентs.Add(student);
                await context.SaveChangesAsync();
            }
            else if (model.UserStructure.UserType == UserType.Teacher)
            {
                Преподаватель teacher = new()
                {
                    IdПользователя = newUser.IdПользователя,
                    IdИнститута = model.UserStructure.IdInstitute!.Value,
                    IdКафедры = model.UserStructure.IdDepartment!.Value,
                    IdДолжности = model.UserStructure.IdPost!.Value,
                    Роли = [rolesOrganization.First(x => x.Название == _settings.RoleTeacherOnVerifyName).IdРоли],
                    Активно = true
                };

                context.Преподавательs.Add(teacher);
                await context.SaveChangesAsync();
            }

            var account = new АккаунтПользователя
            {
                IdПользователя = newUser.IdПользователя,
                Логин = model.Login,
                Пароль = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Роли = []
            };

            return account;
        }

        private async Task<АккаунтПользователя> LinkUserAsync(RegisterModel model, List<Пользователь> candidates)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            АккаунтПользователя account = new()
            {
                Логин = model.Login,
                Пароль = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Роли = []
            };
            List<РольПользователя> roles = await context.РольПользователяs.ToListAsync();

            if (model.UserStructure.UserType == UserType.Student)
            {
                List<Студент> foundStudents = await context.Студентs
                    .Where(x => x.IdИнститута == model.UserStructure.IdInstitute!.Value
                                    && x.IdКафедры == model.UserStructure.IdDepartment!.Value
                                    && x.IdНаправления == model.UserStructure.IdDirection!.Value
                                    && x.IdПрофиля == model.UserStructure.IdProfile
                                    && x.IdУровняОбразования == model.UserStructure.IdEducationLevel!.Value
                                    && x.IdФормыОбучения == model.UserStructure.IdEducationForm!.Value
                                    && x.ГодОкончания == model.UserStructure.YearGraduation!.Value
                                    && x.Активно == true)
                    .ToAsyncEnumerable()
                    .Where(x => candidates.Exists(y => y.IdПользователя == x.IdПользователя))
                    .ToListAsync();

                if (foundStudents.Count > 2)
                {
                    _logger.LogCritical(
                        "Найдено 2 студента одинаковой структуры без аккаунтов ФИО {LastName} {Firstname} {Patromymic}",
                        model.Surname, model.Name, model.Patronymic ?? "");
                }

                if (foundStudents.Count != 0)
                {
                    account.IdПользователя = foundStudents[0].IdПользователя;
                    account.Роли = [roles.First(x => x.Название == _settings.RoleStudentName).IdРоли];
                }
            }
            else if (model.UserStructure.UserType == UserType.Teacher)
            {
                List<Преподаватель> foundTeachers = await context.Преподавательs
                    .Where(x => x.IdИнститута == model.UserStructure.IdInstitute!.Value
                                    && x.IdКафедры == model.UserStructure.IdDepartment!.Value
                                    && x.Активно == true)
                    .ToAsyncEnumerable()
                    .Where(x => candidates.Exists(y => y.IdПользователя == x.IdПользователя))
                    .ToListAsync();

                if (foundTeachers.Count > 2)
                {
                    _logger.LogCritical(
                        "Найдено 2 преподавателя одинаковой структуры без аккаунтов с ФИО {LastName} {Firstname} {Patromymic}",
                        model.Surname, model.Name, model.Patronymic ?? "");
                }

                if (foundTeachers.Count != 0)
                {
                    account.IdПользователя = foundTeachers[0].IdПользователя;
                    account.Роли = [roles.First(x => x.Название == _settings.RoleTeacherName).IdРоли];
                }
            }

            if (account.IdПользователя == 0 || account.Роли.Count == 0)
            {
                account = await CreateUserAsync(model);
            }

            return account;
        }
    }
}
