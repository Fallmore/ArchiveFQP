using ArchiveFqp.Factories.DisplayDto.Student;
using ArchiveFqp.Interfaces;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.Student;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.Settings.SettingsArchive;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Services.Student
{

    public class StudentService : CrudGeneric, IStudentService
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IReferenceDataService _refDataService;
        private readonly SettingsArchive _settings;
        private StudentDtoFactory _factoryStudent;

        public StudentService(IDbContextFactory<ArchiveFqpContext> dbFactory,
            IReferenceDataService refDataService, SettingsArchive settings,
            StudentDtoFactory? factoryStudent = null
            )
        {
            _dbFactory = dbFactory;
            _refDataService = refDataService;
            _settings = settings;
            _factoryStudent = factoryStudent ?? new(refDataService);
        }

        public async Task<Студент?> GetStudentAsync(int idStudent)
        {
            return (await GetStudentsAsync([idStudent])).FirstOrDefault();
        }

        public async Task<List<Студент>> GetStudentsAsync(List<int> idStudents)
        {
            List<Студент> students = await _refDataService.GetAsync<Студент>();
            return students.Where(x => idStudents.Contains(x.IdСтудента)).ToList();
        }

        public async Task<StudentDisplayDto?> GetStudentDisplayAsync(int idStudent)
        {
            Студент? student = await GetStudentAsync(idStudent);
            if (student is null) return null;
            return await GetStudentDisplayAsync(student);
        }

        public async Task<StudentDisplayDto> GetStudentDisplayAsync(Студент student)
        {
            return await _factoryStudent.CreateDisplayDtoAsync(student);
        }

        public async Task<List<StudentDisplayDto>> GetStudentDisplayAsync(List<int> idStudents)
        {
            List<Студент> students = await GetStudentsAsync(idStudents);
            return await GetStudentDisplayAsync(students);
        }

        public async Task<List<StudentDisplayDto>> GetStudentDisplayAsync(List<Студент> students)
        {
            return await _factoryStudent.CreateDisplayDtoAsync(students);
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
            if (typeof(T).Name == nameof(Студент))
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
    }
}
