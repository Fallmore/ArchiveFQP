using ArchiveFqp.Factories.DisplayDto.Structure;
using ArchiveFqp.Factories.DisplayDto.User;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Models.DTO.User;
using ArchiveFqp.Models.ReferenceData;

namespace ArchiveFqp.Factories.DisplayDto.Teacher
{

    public class TeacherDtoFactory : IDisplayDtoFactory<TeacherDisplayDto, Преподаватель>
    {
        private readonly IReferenceDataService _refDataService;
        private readonly StructureDtoFactory _structureDisplayFactory;

        private ReferenceDataSnapshot _snapshot = null!;

        private Task _init;

        public TeacherDtoFactory(IReferenceDataService refDataService, StructureDtoFactory? structureDisplayFactory = null)
        {
            _refDataService = refDataService;
            _structureDisplayFactory = structureDisplayFactory ?? new(_refDataService);
            _init = Task.Run(InitializeLists);
        }

        private async Task InitializeLists()
        {
            _snapshot = await _refDataService.GetSnapshotAsync();
        }

        public async Task<TeacherDisplayDto> CreateDisplayDtoAsync(Преподаватель teacher)
        {
            _init.Wait();
            return new()
            {
                IdПреподавателя = teacher.IdПреподавателя,
                Пользователь = (await _refDataService.GetAsync<UserDisplayDto>()).First(x => x.Пользователь.IdПользователя == teacher.IdПользователя) ?? new(),
                Структура = await _structureDisplayFactory.CreateDisplayDtoAsync<Кафедра>(teacher.IdКафедры) ?? new(),
                Роли = [.. _snapshot.RolesOrganization.Where(x => teacher.Роли.Contains(x.IdРоли))],
                Активно = teacher.Активно,
                Должность = _snapshot.Posts.FirstOrDefault(o => o.IdДолжности == teacher.IdДолжности) ?? new()
            };
        }

        public async Task<TeacherDisplayDto?> CreateDisplayDtoAsync(int id)
        {
            _init.Wait();
            Преподаватель? teacher = _snapshot.Teachers.FirstOrDefault(o => o.IdПреподавателя == id);
            if (teacher == null) return null;

            return await CreateDisplayDtoAsync(teacher);
        }

        /// <summary>
        /// <inheritdoc cref="CreateDisplayDtoAsync(int)"/> и id должности
        /// </summary>
        /// <param name="id"></param>
        /// <param name="idPost">Должность, не зависящая от актуальной должности</param>
        /// <returns></returns>
        public async Task<TeacherDisplayDto> CreateDisplayDtoAsync(int id, int idPost)
        {
            _init.Wait();
            Преподаватель? teacher = _snapshot.Teachers.FirstOrDefault(o => o.IdПреподавателя == id);
            if (teacher == null) return new();

            teacher.IdДолжности = idPost;
            return await CreateDisplayDtoAsync(teacher);
        }

        public async Task<List<TeacherDisplayDto>> CreateDisplayDtoAsync(IEnumerable<Преподаватель> teachers)
        {
            IEnumerable<Task<TeacherDisplayDto>> tasks = teachers.Select(CreateDisplayDtoAsync);
            TeacherDisplayDto[] results = await Task.WhenAll(tasks);
            return [.. results];
        }

        /// <summary>
        /// <inheritdoc cref="CreateDisplayDtoAsync(IEnumerable{Преподаватель})"/> через id без актуальных должностей
        /// </summary>
        /// <param name="teachers"></param>
        /// <param name="useId"></param>
        /// <returns></returns>
        public async Task<List<TeacherDisplayDto>> CreateDisplayDtoAsync(IEnumerable<Преподаватель> teachers, bool useId = false)
        {
            if (!useId) return await CreateDisplayDtoAsync(teachers);

            IEnumerable<Task<TeacherDisplayDto>> tasks = teachers.Select(o => CreateDisplayDtoAsync(o.IdПреподавателя, o.IdДолжности));
            TeacherDisplayDto[] results = await Task.WhenAll(tasks);
            return [.. results];
        }

        /// <summary>
        /// <inheritdoc cref="CreateDisplayDtoAsync(IEnumerable{Преподаватель})"/>
        /// </summary>
        /// <param name="consultants"></param>
        /// <returns></returns>
        public async Task<List<TeacherDisplayDto>> CreateDisplayDtoAsync(IEnumerable<Консультант> consultants)
        {
            IEnumerable<Task<TeacherDisplayDto>> tasks = consultants.Select(o => CreateDisplayDtoAsync(o.IdПреподавателя, o.IdДолжности));
            TeacherDisplayDto[] results = await Task.WhenAll(tasks);
            return [.. results];
        }

        /// <summary>
        /// <inheritdoc cref="CreateDisplayDtoAsync(IEnumerable{Преподаватель})"/>
        /// </summary>
        /// <param name="reviewers"></param>
        /// <returns></returns>
        public async Task<List<TeacherDisplayDto>> CreateDisplayDtoAsync(IEnumerable<Рецензент> reviewers)
        {
            IEnumerable<Task<TeacherDisplayDto>> tasks = reviewers.Select(o => CreateDisplayDtoAsync(o.IdПреподавателя, o.IdДолжности));
            TeacherDisplayDto[] results = await Task.WhenAll(tasks);
            return [.. results];
        }
    }
}
