using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Models.DTO.User;

namespace ArchiveFqp.Services.User
{
    public interface IUserService
    {
        /// <summary>
        /// Получение пользователя типа <see cref="UserDisplayDto"/> для отображения информации
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        Task<UserDisplayDto?> GetUserDisplayAsync(int idUser);
        /// <summary>
        /// <inheritdoc cref="GetUserDisplayAsync"/>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<UserDisplayDto> GetUserDisplayAsync(Пользователь user);

        /// <summary>
        /// Получение пользователя типа <see cref="TeacherDisplayDto"/> для отображения информации
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        Task<TeacherDisplayDto?> GetTeacherDisplayAsync(int idUser);
        /// <summary>
        /// <inheritdoc cref="GetTeacherDisplayAsync"/>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<TeacherDisplayDto?> GetTeacherDisplayAsync(Пользователь user);

        /// <summary>
        /// Получение пользователя типа <see cref="StudentDisplayDto"/> для отображения информации
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        Task<StudentDisplayDto?> GetStudentDisplayAsync(int idUser);
        /// <summary>
        /// <inheritdoc cref="GetStudentDisplayAsync"/>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<StudentDisplayDto?> GetStudentDisplayAsync(Пользователь user);

    }
}