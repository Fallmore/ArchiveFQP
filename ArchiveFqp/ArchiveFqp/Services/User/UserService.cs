using ArchiveFqp.Factories.DisplayDto.Student;
using ArchiveFqp.Factories.DisplayDto.Teacher;
using ArchiveFqp.Factories.DisplayDto.User;
using ArchiveFqp.Interfaces;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.User;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Structure;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Models.DTO.User;
using ArchiveFqp.Models.Settings.SettingsArchive;
using BCrypt.Net;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Services.User
{
    public class UserService : CrudGeneric, IUserService
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IReferenceDataService _refDataService;
        private readonly SettingsArchive _settings;
        private TeacherDtoFactory _factoryTeacher;
        private StudentDtoFactory _factoryStudent;

        public UserService(IDbContextFactory<ArchiveFqpContext> dbFactory,
            IReferenceDataService refDataService, SettingsArchive settings,
            TeacherDtoFactory? factoryTeacher = null, StudentDtoFactory? factoryStudent = null
            )
        {
            _dbFactory = dbFactory;
            _refDataService = refDataService;
            _settings = settings;
            _factoryTeacher = factoryTeacher ?? new(refDataService);
            _factoryStudent = factoryStudent ?? new(refDataService);
        }

        public async Task<Пользователь?> GetUserAsync(int idUser)
        {
            return (await GetUsersAsync()).FirstOrDefault(x => x.IdПользователя == idUser);
        }

        public async Task<List<Пользователь>> GetUsersAsync()
        {
            return await _refDataService.GetAsync<Пользователь>();
        }

        public async Task<List<UserDisplayDto>> GetUsersByRolesAsync(List<string> roles)
        {
            return (await _refDataService.GetAsync<UserDisplayDto>())
                .Where(x => roles.Any(r => x.Роли.Contains(r))).ToList();
        }

        public async Task<АккаунтПользователя?> GetUserAccountAsync(Пользователь user)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();
            АккаунтПользователя? account = await context.АккаунтПользователяs.FirstOrDefaultAsync(x => x.IdПользователя == user.IdПользователя);
            return account;
        }

        public async Task<UserDisplayDto?> GetUserDisplayAsync(int idUser)
        {
            return (await _refDataService.GetAsync<UserDisplayDto>()).FirstOrDefault(x => x.Пользователь.IdПользователя == idUser);
        }

        public async Task<UserDisplayDto> GetUserDisplayAsync(Пользователь user)
        {
            return (await GetUserDisplayAsync(user.IdПользователя))!;
        }

        public async Task<List<UserDisplayDto>> GetUserDisplayAsync(List<int> idUsers)
        {
            return (await _refDataService.GetAsync<UserDisplayDto>()).Where(x => idUsers.Contains(x.Пользователь.IdПользователя)).ToList();
        }

        public async Task<List<UserDisplayDto>> GetUserDisplayAsync(List<Пользователь> users)
        {
            List<int> ids = users.Select(x => x.IdПользователя).ToList();
            return await GetUserDisplayAsync(ids);
        }


        public async Task<Преподаватель?> GetTeacherAsync(int idUser)
        {
            return (await GetTeachersAsync([idUser])).FirstOrDefault();
        }

        public async Task<List<Преподаватель>> GetTeachersAsync(List<int> idUsers)
        {
            List<Преподаватель> teachers = await _refDataService.GetAsync<Преподаватель>();
            List<Преподаватель> result = [];

            foreach (int idUser in idUsers)
            {
                List<Преподаватель> temp = teachers
                    .Where(x => x.IdПользователя == idUser).ToList();
                if (temp.Count == 0) continue;

                // Если пользователь на проверке смены своей работы, то берем предпоследнюю запись преподавателя
                if ((await GetUserDisplayAsync(idUser))!.Роли.Contains(_settings.RoleTeacherOnVerifyName))
                {
                    if (temp.Count == 1) continue;

                    result.Add(temp.ElementAt(Math.Max(0, temp.Count - 2)));
                }

                // Иначе берем последнюю
                result.Add(temp.Last());
            }

            return result;
        }

        public async Task<TeacherDisplayDto?> GetTeacherDisplayAsync(int idUser)
        {
            Преподаватель? teacher = (await _refDataService.GetAsync<Преподаватель>()).FirstOrDefault(x => x.IdПользователя == idUser);
            if (teacher == null) return null;
            return await _factoryTeacher.CreateDisplayDtoAsync(teacher);
        }

        public async Task<TeacherDisplayDto?> GetTeacherDisplayAsync(Пользователь user)
        {
            return await GetTeacherDisplayAsync(user.IdПользователя);
        }

        public async Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(List<int> idUsers)
        {
            List<Преподаватель> teachers = (await _refDataService.GetAsync<Преподаватель>())
                .Where(x => idUsers.Contains(x.IdПользователя))
                .ToList();
            return await _factoryTeacher.CreateDisplayDtoAsync(teachers);
        }

        public async Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(List<Пользователь> users)
        {
            return await GetTeacherDisplayAsync(users.Select(x => x.IdПользователя).ToList());
        }



        public async Task<Студент?> GetStudentAsync(int idUser)
        {
            return (await GetStudentsAsync([idUser])).FirstOrDefault();
        }

        public async Task<List<Студент>> GetStudentsAsync(List<int> idUsers)
        {
            List<Студент> students = await _refDataService.GetAsync<Студент>();
            List<Студент> result = [];

            foreach (int idUser in idUsers)
            {
                List<Студент> temp = students
                    .Where(x => x.IdПользователя == idUser).ToList();
                if (temp.Count == 0) continue;

                // Если пользователь на проверке смены своего обучения, то берем предпоследнюю запись студента
                if ((await GetUserDisplayAsync(idUser))!.Роли.Contains(_settings.RoleStudentOnVerifyName))
                {
                    // Если это единичная запись, то пропускаем
                    if (temp.Count == 1) continue;

                    result.Add(temp.ElementAt(Math.Max(0, temp.Count - 2)));
                }

                // Иначе берем последнюю
                result.Add(temp.Last());
            }

            return result;
        }

        public async Task<StudentDisplayDto?> GetStudentDisplayAsync(int idUser)
        {
            Студент? student = await GetStudentAsync(idUser);
            if (student == null) return null;
            return await _factoryStudent.CreateDisplayDtoAsync(student);
        }

        public async Task<StudentDisplayDto?> GetStudentDisplayAsync(Пользователь user)
        {
            return await GetStudentDisplayAsync(user.IdПользователя);
        }

        public async Task<List<StudentDisplayDto>> GetStudentDisplayAsync(List<int> idUsers)
        {
            List<Студент> students = await GetStudentsAsync(idUsers);
            return await _factoryStudent.CreateDisplayDtoAsync(students);
        }

        public async Task<List<StudentDisplayDto>> GetStudentDisplayAsync(List<Пользователь> users)
        {
            return await GetStudentDisplayAsync(users.Select(x => x.IdПользователя).ToList());
        }

        public async Task<bool> Upsert<T>(T item) where T : class
        {
            return await base.Upsert(item, _dbFactory);
        }

        public async Task<bool> Delete<T>(int id) where T : class
        {
            return await base.Delete<T>(id, _dbFactory);
        }

        public async Task<АккаунтПользователя> AddAccountStudentDefault(StudentDisplayDto student)
        {
            АккаунтПользователя account = new()
            {
                IdПользователя = student.Пользователь.Пользователь.IdПользователя,
                Логин = string.Join(" ", student.Пользователь.ФИОИнициалы, student.ГодОкончания),
                Пароль = BCrypt.Net.BCrypt.HashPassword(string.Join("", student.Пользователь.ФИОИнициалы, StructureDto.Abbreviate(student.Структура.Кафедра.Название), student.ГодОкончания)),
                Роли = [(await _refDataService.GetAsync<РольПользователя>()).First(x => x.Название == _settings.RoleStudentName).IdРоли]
            };

            await Upsert(account);
            return account;
        }
    }
}
