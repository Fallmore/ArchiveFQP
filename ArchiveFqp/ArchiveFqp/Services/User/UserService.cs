using ArchiveFqp.Factories.DisplayDto;
using ArchiveFqp.Factories.DisplayDto.Student;
using ArchiveFqp.Factories.DisplayDto.Teacher;
using ArchiveFqp.Factories.DisplayDto.User;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.User;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Models.DTO.User;
using ArchiveFqp.Services.Work;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;

namespace ArchiveFqp.Services.User
{
    public class UserService : IUserService
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

        public async Task<bool> UpdateUser(Пользователь user)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            Пользователь? foundUser = context.Пользовательs.Find(user.IdПользователя);
            if (foundUser == null) return false;

            foundUser = user;
            await context.SaveChangesAsync();
            return true;
        }
    }
}
