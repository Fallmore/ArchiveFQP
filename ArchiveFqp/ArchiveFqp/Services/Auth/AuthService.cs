using ArchiveFqp.Interfaces.Auth;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Models.Auth;
using ArchiveFqp.Models.Database;
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

        public AuthService(IDbContextFactory<ArchiveFqpContext> dbFactory, IReferenceDataService refDataService,
            IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _dbFactory = dbFactory;
            _refDataService = refDataService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

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

            // Создаем аккаунт с хешированным паролем
            АккаунтПользователя account = new()
            {
                IdПользователя = user.IdПользователя,
                Логин = model.Login,
                Пароль = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Роли = []
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
                    ГодОкончания = model.YearGraduation!.Value
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
                    IdДолжности = model.IdPost!.Value
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

        public async Task LogoutAsync()
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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


    }
}
