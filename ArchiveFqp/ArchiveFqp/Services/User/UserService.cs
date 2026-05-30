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


        public async Task<List<Преподаватель>> GetTeacherAsync(int idUser)
        {
            return (await GetTeachersAsync([idUser])).Where(x => x.IdПользователя == idUser).ToList();
        }

        public async Task<List<Преподаватель>> GetTeachersAsync(List<int> idUsers)
        {
            List<Преподаватель> teachers = await _refDataService.GetAsync<Преподаватель>();
            return teachers.Where(x => idUsers.Contains(x.IdПользователя)).ToList();
        }

        public async Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(int idUser)
        {
            List<Преподаватель> teacher = (await GetTeacherAsync(idUser));
            if (teacher == null) return [];
            return await _factoryTeacher.CreateDisplayDtoAsync(teacher);
        }

        public async Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(Пользователь user)
        {
            return await GetTeacherDisplayAsync(user.IdПользователя);
        }

        public async Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(List<int> idUsers)
        {
            List<Преподаватель> teachers = await GetTeachersAsync(idUsers);
            return await _factoryTeacher.CreateDisplayDtoAsync(teachers);
        }

        public async Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(List<Пользователь> users)
        {
            return await GetTeacherDisplayAsync(users.Select(x => x.IdПользователя).ToList());
        }



        public async Task<List<Студент>> GetStudentAsync(int idUser)
        {
            return (await GetStudentsAsync([idUser])).Where(x => x.IdПользователя == idUser).ToList();
        }

        public async Task<List<Студент>> GetStudentsAsync(List<int> idUsers)
        {
            List<Студент> students = await _refDataService.GetAsync<Студент>();
            return students.Where(x => idUsers.Contains(x.IdПользователя)).ToList();
        }

        public async Task<List<StudentDisplayDto>> GetStudentDisplayAsync(int idUser)
        {
            List<Студент> student = await GetStudentAsync(idUser);
            if (student.Count == 0) return [];
            return await _factoryStudent.CreateDisplayDtoAsync(student);
        }

        public async Task<List<StudentDisplayDto>> GetStudentDisplayAsync(Пользователь user)
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

        public async Task<bool> Delete<T>(int id, bool removeRoles = false) where T : class
        {
            if (removeRoles) await RemoveRoles<T>(id);
            return await base.Delete<T>(id, _dbFactory);
        }

        /// <summary>
        /// Убирает роли без вызова сохранения в БД
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task RemoveRoles<T>(int id) where T : class
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();
            АккаунтПользователя? account = null;
            List<РольПользователя> roles = await _refDataService.GetAsync<РольПользователя>();
            List<string> comparerRoles = [];

            int? idPerson = null;
            List<РольПользователя> personRoles = [];
            List<int> idRolesForDelete = [];

            // TODO: Привязка ролей кафедры к преподавателям, а не к аккаунту
            // TODO: Привязка верификации студента и преподавателя не к аккаунту
            // Как я ужасно сделал роли...
            if (typeof(T).Name == nameof(Преподаватель))
            {
                List<Преподаватель> teachers = (await _refDataService.GetAsync<Преподаватель>());

                Преподаватель teacher = teachers.First(x => x.IdПреподавателя == id);
                idPerson = teacher.IdПользователя;

                teachers = teachers.Where(x => x.IdПользователя == idPerson).ToList();

                if (teachers.Count != 0)
                {
                    account = await context.АккаунтПользователяs.FirstOrDefaultAsync(x => x.IdПользователя == idPerson);
                    if (account is null) return;

                    personRoles = roles.Where(x => account.Роли.Contains(x.IdРоли)).ToList();
                    comparerRoles = [_settings.RoleDepartmentHeadName,
                          _settings.RoleDepartmentResponsibleName,
                          _settings.RoleDepartmentClerkName];
                    
                    if (teachers.Count == 2
                        && personRoles.Exists(x => x.Название == _settings.RoleTeacherOnVerifyName))
                    {
                        if (teacher.Активно)
                            comparerRoles.Add(_settings.RoleTeacherName);
                        if (!teacher.Активно)
                            comparerRoles.Add(_settings.RoleTeacherOnVerifyName);
                    }
                    else if (teachers.Count == 1)
                    {
                        comparerRoles.Add(_settings.RoleTeacherName);
                        comparerRoles.Add(_settings.RoleTeacherOnVerifyName);
                    }

                }
            }
            else if (typeof(T).Name == nameof(Студент))
            {
                List<Студент> students = (await _refDataService.GetAsync<Студент>());
                Студент student = students.First(x => x.IdСтудента == id);
                idPerson = student.IdПользователя;

                students = students.Where(x => x.IdПользователя == idPerson).ToList();

                if (students.Count != 0)
                {
                    account = await context.АккаунтПользователяs.FirstOrDefaultAsync(x => x.IdПользователя == idPerson);
                    if (account is null) return;

                    personRoles = roles.Where(x => account.Роли.Contains(x.IdРоли)).ToList();
                    if (students.Count == 2
                        && personRoles.Exists(x => x.Название == _settings.RoleTeacherOnVerifyName))
                    {
                        if (student.Активно)
                            comparerRoles.Add(_settings.RoleStudentName);
                        if (!student.Активно)
                            comparerRoles.Add(_settings.RoleStudentOnVerifyName);
                    }
                    else if (students.Count == 1)
                    {
                        comparerRoles.Add(_settings.RoleStudentOnVerifyName);
                        comparerRoles.Add(_settings.RoleStudentName);
                    }
                }
            }

            idRolesForDelete = roles.Where(x => comparerRoles.Contains(x.Название))
                .Select(x => x.IdРоли).ToList();

            account?.Роли.RemoveAll(x => idRolesForDelete.Contains(x));
        }

        public async Task<АккаунтПользователя> AddAccountStudentDefault(StudentDisplayDto student)
        {
            АккаунтПользователя account = new()
            {
                IdПользователя = student.Пользователь.Пользователь.IdПользователя,
                Логин = string.Join("", student.Пользователь.ФИОИнициалы, student.ГодОкончания),
                Пароль = BCrypt.Net.BCrypt.HashPassword(string.Join("", student.Пользователь.ФИОИнициалы, StructureDto.Abbreviate(student.Структура.Кафедра.Название), student.ГодОкончания)),
                Роли = [(await _refDataService.GetAsync<РольПользователя>()).First(x => x.Название == _settings.RoleStudentName).IdРоли]
            };

            await Upsert(account);
            return account;
        }

        public async Task<АккаунтПользователя> AddAccountTeacherDefault(TeacherDisplayDto teacher)
        {
            АккаунтПользователя account = new()
            {
                IdПользователя = teacher.Пользователь.Пользователь.IdПользователя,
                Логин = string.Join("", teacher.Пользователь.ФИО, "2026"),
                Пароль = BCrypt.Net.BCrypt.HashPassword(string.Join("", teacher.Пользователь.ФИО, StructureDto.Abbreviate(teacher.Структура.Кафедра.Название), "2026")),
                Роли = [(await _refDataService.GetAsync<РольПользователя>()).First(x => x.Название == _settings.RoleTeacherName).IdРоли]
            };

            await Upsert(account);
            return account;
        }

    }
}
