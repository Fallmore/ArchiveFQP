using ArchiveFqp.Interfaces.Auth;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.User;
using ArchiveFqp.Models.Auth;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.Settings.SettingsArchive;
using ArchiveFqp.Services.Notifications;
using ArchiveFqp.Services.User;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ArchiveFqp.Services.Auth
{

    public class AuthService : IAuthService
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IUserService _userService;
        private readonly SettingsArchive _settings;
        private readonly IReferenceDataService _refDataService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //public event Action<ClaimsPrincipal>? UserChanged;
        //private ClaimsPrincipal? currentUser;

        //public ClaimsPrincipal CurrentUser
        //{
        //    get { return currentUser ?? new(); }
        //    set
        //    {
        //        currentUser = value;

        //        if (UserChanged is not null)
        //        {
        //            UserChanged(currentUser);
        //        }
        //    }
        //}

        public AuthService(IDbContextFactory<ArchiveFqpContext> dbFactory,
            IUserService userService,
            SettingsArchive settings, IReferenceDataService refDataService,
            IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _dbFactory = dbFactory;
            _userService = userService;
            _settings = settings;
            _refDataService = refDataService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        // TODO: добавить валидацию модели регистрации (пароль, почта и т.д.)
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

            // Создаем пользователя
            Пользователь user = new()
            {
                Фамилия = model.Surname!,
                Имя = model.Name!,
                Отчество = model.Patronymic,
                Email = model.Email
            };

            context.Пользовательs.Add(user);
            await context.SaveChangesAsync();

            List<РольПользователя> roles = await _refDataService.GetAsync<РольПользователя>();
            List<int> userRoles = [];
            if (model.UserType == UserType.Student)
                userRoles.Add(roles.First(x => x.Название == _settings.RoleStudentName).IdРоли);
            if (model.UserType == UserType.Teacher)
                userRoles.Add(roles.First(x => x.Название == _settings.RoleTeacherName).IdРоли);

            // Создаем аккаунт с хешированным паролем
            АккаунтПользователя account = new()
            {
                IdПользователя = user.IdПользователя,
                Логин = model.Login,
                Пароль = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Роли = userRoles
            };

            context.АккаунтПользователяs.Add(account);
            await context.SaveChangesAsync();

            if (model.UserType == UserType.Student)
            {
                Студент student = new()
                {
                    IdПользователя = user.IdПользователя,
                    IdИнститута = model.IdInstitute!.Value,
                    IdКафедры = model.IdDepartment!.Value,
                    IdНаправления = model.IdDirection!.Value,
                    IdПрофиля = model.IdProfile,
                    IdУровняОбразования = model.IdEducationLevel!.Value,
                    IdФормыОбучения = model.IdEducationForm!.Value,
                    ГодОкончания = model.YearGraduation!.Value,
                    Роли = [roles.First(x => x.Название == _settings.RoleStudentOnVerifyName).IdРоли],
                    Активно = true
                };

                context.Студентs.Add(student);
                await context.SaveChangesAsync();
            }
            else if (model.UserType == UserType.Teacher)
            {
                Преподаватель teacher = new()
                {
                    IdПользователя = user.IdПользователя,
                    IdИнститута = model.IdInstitute!.Value,
                    IdКафедры = model.IdDepartment!.Value,
                    IdДолжности = model.IdPost!.Value,
                    Роли = [roles.First(x => x.Название == _settings.RoleTeacherOnVerifyName).IdРоли],
                    Активно = true
                };

                context.Преподавательs.Add(teacher);
                await context.SaveChangesAsync();
            }

            return new AuthResult
            {
                Success = true,
                Message = "Регистрация успешна"
                //User = new UserSession
                //{
                //    UserId = user.IdПользователя,
                //    ФИО = $"{user.Фамилия} {user.Имя}",
                //    Логин = model.Логин,
                //    Роли = []
                //}
            };
        }

        public async Task<AuthResult> LoginAsync(LoginModel model)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            АккаунтПользователя? account = await context.АккаунтПользователяs
                .Include(a => a.IdПользователяNavigation)
                .FirstOrDefaultAsync(a => a.Логин == model.Login);

            if (account == null || !BCrypt.Net.BCrypt.Verify(model.Password, account.Пароль))
            {
                return new AuthResult { Success = false, Message = "Неверный логин или пароль" };
            }

            List<string> roleNames = (await _refDataService.GetAsync<РольПользователя>())
                .Where(r => account.Роли.Contains(r.IdРоли))
                .Select(r => r.Название)
                .ToList();
            // Совмещаем роли, т.к. использование политик очень сильно усложнит мне жизнь.
            // И я надеюсь, что данное решение не усложнит вашу
            roleNames.AddRange(await GetRoleNamesOrganizaion(account.IdПользователя));

            List<Claim> claims = [
                new Claim(ClaimTypes.NameIdentifier, account.IdПользователя.ToString()),
                new Claim(ClaimTypes.Name, $"{account.IdПользователяNavigation.Фамилия} {account.IdПользователяNavigation.Имя}"),
                ];

            // Добавляем роли в claims
            foreach (string role in roleNames)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new(identity);

            await _httpContextAccessor.HttpContext!.SignInAsync(principal);
            //CookieAuthenticationDefaults.AuthenticationScheme,
            //CurrentUser,
            //new AuthenticationProperties
            //{
            //    IsPersistent = true,
            //    ExpiresUtc = DateTime.UtcNow.AddHours(8)
            //});

            //string token = GenerateJwtToken(userSession);

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

        private async Task<List<string>> GetRoleNamesOrganizaion(int userId)
        {
            List<РольУчреждения> roles = await _refDataService.GetAsync<РольУчреждения>();

            Преподаватель? teacher = (await _userService.GetTeacherAsync(userId))
                .FirstOrDefault(x => x.Активно == true
                && !x.Роли.Contains(roles.First(y => y.Название == _settings.RoleStudentOnVerifyName).IdРоли));
            Студент? student = (await _userService.GetStudentAsync(userId))
                .FirstOrDefault(x => x.Активно == true
                && !x.Роли.Contains(roles.First(y => y.Название == _settings.RoleTeacherOnVerifyName).IdРоли));

            if (teacher == null && student == null) return [];

            List<int> roleIds = [];
            roleIds.AddRange(teacher?.Роли ?? []);
            roleIds.AddRange(student?.Роли ?? []);
            roleIds = [.. roleIds.Distinct()];
            return (await _refDataService.GetAsync<РольУчреждения>())
                .Where(r => roleIds.Contains(r.IdРоли))
                .Select(r => r.Название)
                .ToList();
        }
    }
}
