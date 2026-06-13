using ArchiveFqp.Models.Database;
using ArchiveFqp.Interfaces;
using ArchiveFqp.Models.DTO.Teacher;

namespace ArchiveFqp.Interfaces.Teacher
{
    public interface ITeacherService
    {
        Task<Преподаватель?> GetTeacherAsync(int idTeacher);
        Task<List<Преподаватель>> GetTeachersAsync(List<int> idTeachers);

        /// <summary>
        /// Получение пользователя типа <see cref="TeacherDisplayDto"/> по фильтру по ролям
        /// </summary>
        /// <param name="roles">Список ролей</param>
        /// <returns>Список пользователей</returns>
        Task<List<Преподаватель>> GetTeachersByRolesAsync(List<string> roles);

        /// <summary>
        /// Получение преподавателя типа <see cref="TeacherDisplayDto"/> для отображения информации
        /// </summary>
        /// <param name="idTeacher"></param>
        /// <returns></returns>
        Task<TeacherDisplayDto?> GetTeacherDisplayAsync(int idTeacher);
        /// <inheritdoc cref="GetTeacherDisplayAsync"/>
        Task<TeacherDisplayDto> GetTeacherDisplayAsync(Преподаватель teacher);
        /// <summary>
        /// Получение списка преподавателей типа <see cref="TeacherDisplayDto"/> для отображения информации
        /// </summary>
        Task<List<TeacherDisplayDto>> GetTeachersDisplayAsync(List<int> idTeachers);
        /// <inheritdoc cref="GetTeacherDisplayAsync"/>
        Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(List<Преподаватель> teachers);

        /// <summary> <inheritdoc cref="CrudGeneric.Upsert"/> </summary>
        /// <returns><inheritdoc cref="CrudGeneric.Upsert"/></returns>
        public Task<bool> Upsert<T>(T item) where T : class;
        /// <summary> <inheritdoc cref="CrudGeneric.Delete"/> </summary>
        /// <returns><inheritdoc cref="CrudGeneric.Delete"/></returns>
        public Task<bool> Delete<T>(int id, bool removeRoles = false) where T : class;
    }
}
