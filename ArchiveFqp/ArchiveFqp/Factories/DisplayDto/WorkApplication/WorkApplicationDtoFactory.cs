using ArchiveFqp.Factories.DisplayDto.Student;
using ArchiveFqp.Factories.DisplayDto.Teacher;
using ArchiveFqp.Factories.DisplayDto.Work;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.User;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Work;
using ArchiveFqp.Models.DTO.WorkApplication;
using ArchiveFqp.Models.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Factories.DisplayDto.WorkApplication
{
    public class WorkApplicationDtoFactory : IDisplayDtoFactory<WorkApplicationDto, ЗаявлениеРаботы>
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IReferenceDataService _refDataService;
        private readonly IUserService _userService;
        private readonly WorkDtoFactory _workDtoFactory;
        private readonly StudentDtoFactory _studentDtoFactory;
        private readonly TeacherDtoFactory _teacherDtoFactory;

        private List<Пользователь> _users = [];
        private List<ЗаявлениеРаботы> _workApplications = [];
        private ReferenceDataSnapshot _snapshot = null!;

        private Task _init;

        public WorkApplicationDtoFactory(IDbContextFactory<ArchiveFqpContext> dbFactory,
            IUserService userService, IReferenceDataService refDataService)
        {
            _dbFactory = dbFactory;
            _userService = userService;
            _refDataService = refDataService;
            _studentDtoFactory = new(refDataService);
            _teacherDtoFactory = new(refDataService);
            _workDtoFactory = new(_dbFactory, refDataService,
                _studentDtoFactory, _teacherDtoFactory);

            _init = Task.Run(InitializeLists);
        }

        private async Task InitializeLists()
        {
            _users = await _refDataService.GetAsync<Пользователь>();
            _snapshot = await _refDataService.GetSnapshotAsync();
        }

        public async Task<WorkApplicationDto> CreateDisplayDtoAsync(ЗаявлениеРаботы wApps)
        {
            return await CreateDisplayDtoAsync(wApps, (await _workDtoFactory.CreateDisplayDtoAsync(wApps.IdРаботы))!);
        }

        /// <summary>
        /// <inheritdoc cref="CreateDisplayDtoAsync"/> без загрузки данных о работе,
        /// для оптимизации при отображении списков заявок
        /// </summary>
        /// <param name="wApps"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        public async Task<WorkApplicationDto> CreateDisplayDtoAsync(ЗаявлениеРаботы wApps, WorkDisplayDto work)
        {
            _init.Wait();
            Студент? student = (await _userService.GetStudentAsync(wApps.IdПользователя)).FirstOrDefault(x => x.Активно == true);
            Преподаватель? teacher = (await _userService.GetTeacherAsync(wApps.IdПользователя)).FirstOrDefault(x => x.Активно == true);
            return new()
            {
                IdЗаявления = wApps.IdЗаявления,
                IdРаботы = wApps.IdРаботы,
                Работа = work,
                IdПользователя = wApps.IdПользователя,
                ПользовательСтудент = student != null ? await _studentDtoFactory.CreateDisplayDtoAsync(student) : null,
                ПользовательПреподаватель = teacher != null ? await _teacherDtoFactory.CreateDisplayDtoAsync(teacher) : null,
                Цель = wApps.Цель,
                IdСтатуса = wApps.IdСтатуса,
                Статус = _snapshot.ApplicationStatuses.FirstOrDefault(o => o.IdСтатуса == wApps.IdСтатуса)?.Название ?? "",
                ДатаПоступления = wApps.ДатаПоступления,
                ДатаВозврПоЗаявл = wApps.ДатаВозврПоЗаявл,
                Ответ = wApps.Ответ,
                ДатаОтвета = wApps.ДатаОтвета,
                ДатаВозврПоФакту = wApps.ДатаВозврПоФакту
            };
        }

        public async Task<WorkApplicationDto?> CreateDisplayDtoAsync(int id)
        {
            _init.Wait();
            if (_workApplications.Count == 0) _workApplications = await _refDataService.GetAsync<ЗаявлениеРаботы>();
            ЗаявлениеРаботы? wApps = _workApplications.FirstOrDefault(o => o.IdЗаявления == id);
            if (wApps == null) return null;

            return await CreateDisplayDtoAsync(wApps);
        }

        public async Task<List<WorkApplicationDto>> CreateDisplayDtoAsync(IEnumerable<ЗаявлениеРаботы> wApps)
        {
            IEnumerable<Task<WorkApplicationDto>> tasks = wApps.Select(CreateDisplayDtoAsync);
            WorkApplicationDto[] results = await Task.WhenAll(tasks);
            return results.ToList();
        }
    }
}
