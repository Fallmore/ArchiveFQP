using ArchiveFqp.Factories.DisplayDto;
using ArchiveFqp.Factories.DisplayDto.Structure;
using ArchiveFqp.Factories.DisplayDto.User;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Services.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Factories.DisplayDto.Teacher
{

    public class TeacherDtoFactory : IDisplayDtoFactory<TeacherDisplayDto, Преподаватель>
    {
        private readonly IReferenceDataService _refDataService;
        private readonly UserDtoFactory _userDisplayFactory;
        private readonly StructureDtoFactory _structureDisplayFactory;

        private List<Преподаватель> _teachers = [];
        private List<Должность> _posts = [];

        private Task _init;

        public TeacherDtoFactory(IReferenceDataService refDataService,
            UserDtoFactory? userDisplayFactory = null, StructureDtoFactory? structureDisplayFactory = null)
        {
            _refDataService = refDataService;
            _userDisplayFactory = userDisplayFactory ?? new(_refDataService);
            _structureDisplayFactory = structureDisplayFactory ?? new(_refDataService);
            _init = Task.Run(InitializeLists);
        }

        private async Task InitializeLists()
        {
            _posts = await _refDataService.GetAsync<Должность>();
        }

        public async Task<TeacherDisplayDto> CreateDisplayDtoAsync(Преподаватель teacher)
        {
            _init.Wait();
            return new()
            {
                Пользователь = await _userDisplayFactory.CreateDisplayDtoAsync(teacher.IdПользователя) ?? new(),
                Структура = await _structureDisplayFactory.CreateDisplayDtoAsync<Кафедра>(teacher.IdКафедры) ?? new(),
                Должность = _posts.FirstOrDefault(o => o.IdДолжности == teacher.IdДолжности)?.Название ?? ""
            };
        }

        public async Task<TeacherDisplayDto?> CreateDisplayDtoAsync(int id)
        {
            _init.Wait();
            if (_teachers.Count == 0) _teachers = await _refDataService.GetAsync<Преподаватель>();
            Преподаватель? teacher = _teachers.FirstOrDefault(o => o.IdПреподавателя == id);
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
            if (_teachers.Count == 0) _teachers = await _refDataService.GetAsync<Преподаватель>();
            Преподаватель? teacher = _teachers.FirstOrDefault(o => o.IdПреподавателя == id);
            if (teacher == null) return new();

            teacher.IdДолжности = idPost;
            return await CreateDisplayDtoAsync(teacher);
        }

        public async Task<List<TeacherDisplayDto>> CreateDisplayDtoListAsync(IEnumerable<Преподаватель> teachers)
        {
            IEnumerable<Task<TeacherDisplayDto>> tasks = teachers.Select(CreateDisplayDtoAsync);
            TeacherDisplayDto[] results = await Task.WhenAll(tasks);
            return [.. results];
        }

        /// <summary>
        /// <inheritdoc cref="CreateDisplayDtoListAsync"/> через id без актуальных должностей
        /// </summary>
        /// <param name="teachers"></param>
        /// <param name="useId"></param>
        /// <returns></returns>
        public async Task<List<TeacherDisplayDto>> CreateDisplayDtoListAsync(IEnumerable<Преподаватель> teachers, bool useId = false)
        {
            if (!useId) return await CreateDisplayDtoListAsync(teachers);

            IEnumerable<Task<TeacherDisplayDto>> tasks = teachers.Select(o => CreateDisplayDtoAsync(o.IdПреподавателя, o.IdДолжности));
            TeacherDisplayDto[] results = await Task.WhenAll(tasks);
            return [.. results];
        }

        /// <summary>
        /// <inheritdoc cref="CreateDisplayDtoListAsync"/>
        /// </summary>
        /// <param name="consultants"></param>
        /// <returns></returns>
        public async Task<List<TeacherDisplayDto>> CreateDisplayDtoListAsync(IEnumerable<Консультант> consultants)
        {
            IEnumerable<Task<TeacherDisplayDto>> tasks = consultants.Select(o => CreateDisplayDtoAsync(o.IdПреподавателя, o.IdДолжности));
            TeacherDisplayDto[] results = await Task.WhenAll(tasks);
            return [.. results];
        }

        /// <summary>
        /// <inheritdoc cref="CreateDisplayDtoListAsync"/>
        /// </summary>
        /// <param name="reviewers"></param>
        /// <returns></returns>
        public async Task<List<TeacherDisplayDto>> CreateDisplayDtoListAsync(IEnumerable<Рецензент> reviewers)
        {
            IEnumerable<Task<TeacherDisplayDto>> tasks = reviewers.Select(o => CreateDisplayDtoAsync(o.IdПреподавателя, o.IdДолжности));
            TeacherDisplayDto[] results = await Task.WhenAll(tasks);
            return [.. results];
        }
    }
}
