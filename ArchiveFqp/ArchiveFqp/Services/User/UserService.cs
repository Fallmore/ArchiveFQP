using ArchiveFqp.Factories.DisplayDto.Student;
using ArchiveFqp.Factories.DisplayDto.Teacher;
using ArchiveFqp.Factories.DisplayDto.User;
using ArchiveFqp.Interfaces;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.User;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Models.DTO.User;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Services.User
{
    public class UserService : CrudGeneric, IUserService
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IReferenceDataService _refDataService;
        private UserDtoFactory _factoryUser;
        private TeacherDtoFactory _factoryTeacher;
        private StudentDtoFactory _factoryStudent;

        public UserService(IDbContextFactory<ArchiveFqpContext> dbFactory,
            IReferenceDataService refDataService, UserDtoFactory? factoryUser = null,
            TeacherDtoFactory? factoryTeacher = null, StudentDtoFactory? factoryStudent = null
            )
        {
            _dbFactory = dbFactory;
            _refDataService = refDataService;
            _factoryUser = factoryUser ?? new(refDataService);
            _factoryTeacher = factoryTeacher ?? new(refDataService);
            _factoryStudent = factoryStudent ?? new(refDataService);
        }

        public async Task<Пользователь?> GetUserAsync(int idUser)
        {
            return (await _refDataService.GetAsync<Пользователь>()).FirstOrDefault(x => x.IdПользователя == idUser);
        }

        public async Task<АккаунтПользователя?> GetUserAccountAsync(Пользователь user)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();
            АккаунтПользователя? account = await context.АккаунтПользователяs.FindAsync(user.IdПользователя);
            return account;
        }

        public async Task<UserDisplayDto?> GetUserDisplayAsync(int idUser)
        {
            return await _factoryUser.CreateDisplayDtoAsync(idUser);
        }

        public async Task<UserDisplayDto> GetUserDisplayAsync(Пользователь user)
        {
            return await _factoryUser.CreateDisplayDtoAsync(user);
        }

        public async Task<List<UserDisplayDto>> GetUserDisplayAsync(List<int> idUsers)
        {
            List<Пользователь> users = (await _refDataService.GetAsync<Пользователь>())
                .Where(x => idUsers.Contains(x.IdПользователя))
                .ToList();
            return await GetUserDisplayAsync(users);
        }

        public async Task<List<UserDisplayDto>> GetUserDisplayAsync(List<Пользователь> users)
        {
            return await _factoryUser.CreateDisplayDtoAsync(users);
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

        public async Task<StudentDisplayDto?> GetStudentDisplayAsync(int idUser)
        {
            Студент? student = (await _refDataService.GetAsync<Студент>()).FirstOrDefault(x => x.IdПользователя == idUser);
            if (student == null) return null;
            return await _factoryStudent.CreateDisplayDtoAsync(student);
        }

        public async Task<StudentDisplayDto?> GetStudentDisplayAsync(Пользователь user)
        {
            return await GetStudentDisplayAsync(user.IdПользователя);
        }

        public async Task<List<StudentDisplayDto>> GetStudentDisplayAsync(List<int> idUsers)
        {
            List<Студент> students = (await _refDataService.GetAsync<Студент>())
                .Where(x => idUsers.Contains(x.IdПользователя))
                .ToList();
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
    }
}
