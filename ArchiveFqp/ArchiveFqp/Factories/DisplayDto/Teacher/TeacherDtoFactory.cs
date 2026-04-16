using ArchiveFqp.Factories.DisplayDto;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Services.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Factories.DisplayDto.Teacher
{

    public class TeacherDtoFactory : IDisplayDtoFactory<TeacherDisplayDto, Преподаватель>
    {
        private readonly IReferenceDataService _refDataService;

        private List<Пользователь> _users = [];
        private List<Преподаватель> _teachers = [];
        private List<Институт> _institutes = [];
        private List<Кафедра> _departments = [];
        private List<Должность> _posts = [];

        private Task _init;

        public TeacherDtoFactory(IReferenceDataService refDataService)
        {
            _refDataService = refDataService;
            _init = Task.Run(InitializeLists);
        }

        private async Task InitializeLists()
        {
            _users = await _refDataService.GetAsync<Пользователь>();
            _institutes = await _refDataService.GetAsync<Институт>();
            _departments = await _refDataService.GetAsync<Кафедра>();
            _posts = await _refDataService.GetAsync<Должность>();
        }

        public async Task<TeacherDisplayDto> CreateDisplayDtoAsync(Преподаватель teacher)
        {
            _init.Wait();
            return new()
            {
                Пользователь = _users.FirstOrDefault(o => o.IdПользователя == teacher.IdПользователя) ?? new(),
                Институт = _institutes.FirstOrDefault(o => o.IdИнститута == teacher.IdИнститута)?.Название ?? "",
                Кафедра = _departments.FirstOrDefault(o => o.IdИнститута == teacher.IdИнститута)?.Название ?? "",
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
