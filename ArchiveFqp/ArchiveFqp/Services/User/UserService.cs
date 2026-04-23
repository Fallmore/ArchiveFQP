using ArchiveFqp.Factories.DisplayDto;
using ArchiveFqp.Factories.DisplayDto.Student;
using ArchiveFqp.Factories.DisplayDto.Teacher;
using ArchiveFqp.Factories.DisplayDto.User;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Models.DTO.User;
using ArchiveFqp.Services.ReferenceData;
using ArchiveFqp.Services.Work;
using Microsoft.EntityFrameworkCore;

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
            return await _factoryTeacher.CreateDisplayDtoAsync(idUser);
        }

        public async Task<TeacherDisplayDto?> GetTeacherDisplayAsync(Пользователь user)
        {
            return await _factoryTeacher.CreateDisplayDtoAsync(user.IdПользователя);
        }

        public async Task<StudentDisplayDto?> GetStudentDisplayAsync(int idUser)
        {
            return await _factoryStudent.CreateDisplayDtoAsync(idUser);
        }

        public async Task<StudentDisplayDto?> GetStudentDisplayAsync(Пользователь user)
        {
            return await _factoryStudent.CreateDisplayDtoAsync(user.IdПользователя);
        }
    }
}
