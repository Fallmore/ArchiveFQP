using ArchiveFqp.Interfaces;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;

namespace ArchiveFqp.Interfaces.Student
{
    public interface IStudentService
    {
        Task<Студент?> GetStudentAsync(int idStudent);
        Task<List<Студент>> GetStudentsAsync(List<int> idStudents);
        /// <summary>
        /// Получение студента типа <see cref="StudentDisplayDto"/> для отображения информации
        /// </summary>
        /// <param name="idStudent"></param>
        /// <returns></returns>
        Task<StudentDisplayDto?> GetStudentDisplayAsync(int idStudent);
        /// <inheritdoc cref="GetStudentDisplayAsync"/>
        Task<StudentDisplayDto> GetStudentDisplayAsync(Студент student);
        /// <summary>
        /// Получение списка студентов типа <see cref="StudentDisplayDto"/> для отображения информации
        /// </summary>
        Task<List<StudentDisplayDto>> GetStudentsDisplayAsync(List<int> idStudents);
        /// <inheritdoc cref="GetStudentDisplayAsync"/>
        Task<List<StudentDisplayDto>> GetStudentDisplayAsync(List<Студент> students);

        /// <summary> <inheritdoc cref="CrudGeneric.Upsert"/> </summary>
        /// <returns><inheritdoc cref="CrudGeneric.Upsert"/></returns>
        public Task<bool> Upsert<T>(T item) where T : class;
        /// <summary> <inheritdoc cref="CrudGeneric.Delete"/> </summary>
        /// <returns><inheritdoc cref="CrudGeneric.Delete"/></returns>
        public Task<bool> Delete<T>(int id, bool removeRoles = false) where T : class;
    }
}
