using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Models.DTO.User;

namespace ArchiveFqp.Interfaces.User
{
    public interface IUserService
    {
        /// <summary>
        /// Получение пользователей
        /// </summary>
        /// <returns>Список пользователей</returns>
        Task<List<Пользователь>> GetUsersAsync();

        /// <summary>
        /// Получение пользователя типа <see cref="UserDisplayDto"/> по фильтру по ролям
        /// </summary>
        /// <param name="roles">Список ролей</param>
        /// <returns>Список пользователей</returns>
        Task<List<UserDisplayDto>> GetUsersByRolesAsync(List<string> roles);

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
        Task<АккаунтПользователя?> GetUserAccountAsync(int idUser);
        /// <summary>
        /// Получение аккаунта пользователя
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<АккаунтПользователя?> GetUserAccountAsync(Пользователь user);

        /// <summary>
        /// Получение всех ролей пользователя, в том числе и ролей организации
        /// </summary>
        /// <param name="idUser"></param>
        /// <param name="includeVerifying">Получить ли роли проверки, если есть</param>
        /// <returns></returns>
        Task<List<string>?> GetUserRoleNames(int idUser, bool includeVerifying = false);

        /// <summary>
        /// Получение всех ролей пользователя, в том числе и ролей организации
        /// </summary>
        /// <param name="account"></param>
        /// <param name="includeVerifying">Получить ли роли проверки, если есть</param>
        /// <returns></returns>
        Task<List<string>> GetUserRoleNames(АккаунтПользователя account, bool includeVerifying = false);

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
        /// Получение всех записей преподавателей у пользователя
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        Task<List<Преподаватель>> GetTeacherAsync(int idUser);
        /// <summary>
        /// Получение списка всех записей преподавателей у пользователей
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        Task<List<Преподаватель>> GetTeachersAsync(List<int> idUsers);
        /// <summary>
        /// Получение пользователя типа <see cref="TeacherDisplayDto"/> для отображения информации
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(int idUser);
        /// <inheritdoc cref="GetTeacherDisplayAsync"/>
        Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(Пользователь user);
        /// <inheritdoc cref="GetTeacherDisplayAsync"/>
        Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(List<int> idUsers);
        /// <inheritdoc cref="GetTeacherDisplayAsync"/>
        Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(List<Пользователь> users);


        /// <summary>
        /// Получение всех записей студентов у пользователя
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        Task<List<Студент>> GetStudentAsync(int idUser);
        /// <summary>
        /// Получение списка всех записей студентов у пользователей
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        Task<List<Студент>> GetStudentsAsync(List<int> idUsers);
        /// <summary>
        /// Получение студентов типа <see cref="StudentDisplayDto"/> через пользователя
        /// для отображения информации
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        Task<List<StudentDisplayDto>> GetStudentDisplayAsync(int idUser);
        /// <inheritdoc cref="GetStudentDisplayAsync"/>
        Task<List<StudentDisplayDto>> GetStudentDisplayAsync(Пользователь user);
        /// <inheritdoc cref="GetStudentDisplayAsync"/>
        Task<List<StudentDisplayDto>> GetStudentDisplayAsync(List<int> idUsers);
        /// <inheritdoc cref="GetStudentDisplayAsync"/>
        Task<List<StudentDisplayDto>> GetStudentDisplayAsync(List<Пользователь> users);

        /// <summary> <inheritdoc cref="CrudGeneric.Upsert"/> </summary>
        /// <returns><inheritdoc cref="CrudGeneric.Upsert"/></returns>
        public Task<bool> Upsert<T>(T item) where T : class;
        /// <summary> <inheritdoc cref="CrudGeneric.Delete"/> </summary>
        /// <returns><inheritdoc cref="CrudGeneric.Delete"/></returns>
        public Task<bool> Delete<T>(int id, bool removeRoles = false) where T : class;

        /// <summary>
        /// Добавляет аккаунт пользователя в БД с дефолтными данными:
        /// <br/>Логин: "Фамилия+инициалы+год окончания"
        /// <br/>Пароль: "Фамилия+инициалы+аббревиатуры кафедры+год окончания"
        /// <br/>Роль: Студент
        /// </summary>
        /// <param name="student"></param>
        /// <returns>Аккаунт пользователя</returns>
        public Task<АккаунтПользователя> AddAccountStudentDefault(StudentDisplayDto student);

        /// <summary>
        /// Добавляет аккаунт пользователя в БД с дефолтными данными:
        /// <br/>Логин: "ФИО+2026"
        /// <br/>Пароль: "ФИО+аббревиатуры кафедры+2026"
        /// <br/>Роль: Преподаватель
        /// </summary>
        /// <param name="teacher"></param>
        /// <returns>Аккаунт пользователя</returns>
        public Task<АккаунтПользователя> AddAccountTeacherDefault(TeacherDisplayDto teacher);
    }
}