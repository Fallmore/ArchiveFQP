using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Models.DTO.User;

namespace ArchiveFqp.Interfaces.User
{
    public interface IUserService
    {
        /// <summary>
        /// Получение пользователя
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        Task<Пользователь?> GetUserAsync(int idUser);

        /// <summary>
        /// Получение аккаунта пользователя
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        Task<АккаунтПользователя?> GetUserAccountAsync(Пользователь user);

        /// <summary>
        /// Получение пользователя типа <see cref="UserDisplayDto"/> для отображения информации
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        Task<UserDisplayDto?> GetUserDisplayAsync(int idUser);
        /// <inheritdoc cref="GetUserDisplayAsync"/>
        Task<UserDisplayDto> GetUserDisplayAsync(Пользователь user);
        /// <inheritdoc cref="GetUserDisplayAsync"/>
        Task<List<UserDisplayDto>> GetUserDisplayAsync(List<int> idUsers);
        /// <inheritdoc cref="GetUserDisplayAsync"/>
        Task<List<UserDisplayDto>> GetUserDisplayAsync(List<Пользователь> users);

        /// <summary>
        /// Получение пользователя типа <see cref="TeacherDisplayDto"/> для отображения информации
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        Task<TeacherDisplayDto?> GetTeacherDisplayAsync(int idUser);
        /// <inheritdoc cref="GetTeacherDisplayAsync"/>
        Task<TeacherDisplayDto?> GetTeacherDisplayAsync(Пользователь user);
        /// <inheritdoc cref="GetTeacherDisplayAsync"/>
        Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(List<int> idUsers);
        /// <inheritdoc cref="GetTeacherDisplayAsync"/>
        Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(List<Пользователь> users);

        /// <summary>
        /// Получение пользователя типа <see cref="StudentDisplayDto"/> для отображения информации
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        Task<StudentDisplayDto?> GetStudentDisplayAsync(int idUser);
        /// <inheritdoc cref="GetStudentDisplayAsync"/>
        Task<StudentDisplayDto?> GetStudentDisplayAsync(Пользователь user);
        /// <inheritdoc cref="GetStudentDisplayAsync"/>
        Task<List<StudentDisplayDto>> GetStudentDisplayAsync(List<int> idUsers);
        /// <inheritdoc cref="GetStudentDisplayAsync"/>
        Task<List<StudentDisplayDto>> GetStudentDisplayAsync(List<Пользователь> users);

        /// <summary> <inheritdoc cref="CrudGeneric.Upsert"/> </summary>
        /// <returns><inheritdoc cref="CrudGeneric.Upsert"/></returns>
        public Task<bool> Upsert<T>(T item) where T : class;
        /// <summary> <inheritdoc cref="CrudGeneric.Delete"/> </summary>
        /// <returns><inheritdoc cref="CrudGeneric.Delete"/></returns>
        public Task<bool> Delete<T>(int id) where T : class;
    }
}