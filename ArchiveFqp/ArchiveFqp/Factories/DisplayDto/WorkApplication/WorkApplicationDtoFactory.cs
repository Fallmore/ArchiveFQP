using ArchiveFqp.Factories.DisplayDto.Student;
using ArchiveFqp.Factories.DisplayDto.Teacher;
using ArchiveFqp.Factories.DisplayDto.Work;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.Work;
using ArchiveFqp.Models.DTO.WorkApplication;
using ArchiveFqp.Services.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Factories.DisplayDto.WorkApplication
{
    public class WorkApplicationDtoFactory : IDisplayDtoFactory<WorkApplicationDto, ВыдачаРаботы>
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IReferenceDataService _refDataService;
        private readonly WorkDtoFactory _workDtoFactory;
        private readonly StudentDtoFactory _studentDtoFactory;
        private readonly TeacherDtoFactory _teacherDtoFactory;

        private List<Пользователь> _users = [];
        private List<Студент> _students = [];
        private List<Преподаватель> _teachers = [];
        private List<ВыдачаРаботы> _workApplications = [];
        private List<СтатусВыдачи> _workApplicationStatus = [];

        private Task _init;

        public WorkApplicationDtoFactory(IDbContextFactory<ArchiveFqpContext> dbFactory,
            IReferenceDataService refDataService)
        {
            _dbFactory = dbFactory;
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
            _students = await _refDataService.GetAsync<Студент>();
            _teachers = await _refDataService.GetAsync<Преподаватель>();
            _workApplicationStatus = await _refDataService.GetAsync<СтатусВыдачи>();
        }

        public async Task<WorkApplicationDto> CreateDisplayDtoAsync(ВыдачаРаботы wApps)
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
        public async Task<WorkApplicationDto> CreateDisplayDtoAsync(ВыдачаРаботы wApps, WorkDisplayDto work)
        {
            _init.Wait();
            Студент? student = _students.FirstOrDefault(o => o.IdПользователя == wApps.IdПользователя);
            Преподаватель? teacher = _teachers.FirstOrDefault(o => o.IdПользователя == wApps.IdПользователя);
            return new()
            {
                IdВыдачи = wApps.IdВыдачи,
                IdРаботы = wApps.IdРаботы,
                Работа = work,
                IdПользователя = wApps.IdПользователя,
                ПользовательСтудент = student != null ? await _studentDtoFactory.CreateDisplayDtoAsync(student) : null,
                ПользовательПреподаватель = teacher != null ? await _teacherDtoFactory.CreateDisplayDtoAsync(teacher) : null,
                Цель = wApps.Цель,
                IdСтатусаВыдачи = wApps.IdСтатусаВыдачи,
                СтатусВыдачи = _workApplicationStatus.FirstOrDefault(o => o.IdСтатусаВыдачи == wApps.IdСтатусаВыдачи)?.Название ?? "",
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
            if (_workApplications.Count == 0) _workApplications = await _refDataService.GetAsync<ВыдачаРаботы>();
            ВыдачаРаботы? wApps = _workApplications.FirstOrDefault(o => o.IdВыдачи == id);
            if (wApps == null) return null;

            return await CreateDisplayDtoAsync(wApps);
        }

        public async Task<List<WorkApplicationDto>> CreateDisplayDtoListAsync(IEnumerable<ВыдачаРаботы> wApps)
        {
            IEnumerable<Task<WorkApplicationDto>> tasks = wApps.Select(CreateDisplayDtoAsync);
            WorkApplicationDto[] results = await Task.WhenAll(tasks);
            return results.ToList();
        }
    }
}
