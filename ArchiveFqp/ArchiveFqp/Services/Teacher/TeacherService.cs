using ArchiveFqp.Factories.DisplayDto.Teacher;
using ArchiveFqp.Interfaces;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.Teacher;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Models.Settings.SettingsArchive;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Services.Teacher
{
    public class TeacherService : CrudGeneric, ITeacherService
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IReferenceDataService _refDataService;
        private readonly SettingsArchive _settings;
        private TeacherDtoFactory _factoryTeacher;

        public TeacherService(IDbContextFactory<ArchiveFqpContext> dbFactory,
            IReferenceDataService refDataService, SettingsArchive settings,
            TeacherDtoFactory? factoryTeacher = null
            )
        {
            _dbFactory = dbFactory;
            _refDataService = refDataService;
            _settings = settings;
            _factoryTeacher = factoryTeacher ?? new(refDataService);
        }

        public async Task<Преподаватель?> GetTeacherAsync(int idTeacher)
        {
            return (await GetTeachersAsync([idTeacher])).FirstOrDefault();
        }

        public async Task<List<Преподаватель>> GetTeachersAsync(List<int> idTeachers)
        {
            List<Преподаватель> teachers = await _refDataService.GetAsync<Преподаватель>();
            return teachers.Where(x => idTeachers.Contains(x.IdПреподавателя)).ToList();

        }

        public async Task<TeacherDisplayDto?> GetTeacherDisplayAsync(int idTeacher)
        {
            Преподаватель? teacher = (await GetTeacherAsync(idTeacher));
            if (teacher is null) return null;
            return await GetTeacherDisplayAsync(teacher);
        }

        public async Task<TeacherDisplayDto> GetTeacherDisplayAsync(Преподаватель teacher)
        {
            return await _factoryTeacher.CreateDisplayDtoAsync(teacher);
        }

        public async Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(List<int> idTeachers)
        {
            List<Преподаватель> teachers = await GetTeachersAsync(idTeachers);
            return await GetTeacherDisplayAsync(teachers);
        }

        public async Task<List<TeacherDisplayDto>> GetTeacherDisplayAsync(List<Преподаватель> teachers)
        {
            return await _factoryTeacher.CreateDisplayDtoAsync(teachers);
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

            idRolesForDelete = roles.Where(x => comparerRoles.Contains(x.Название))
                .Select(x => x.IdРоли).ToList();

            account?.Роли.RemoveAll(x => idRolesForDelete.Contains(x));
        }
    }
}
