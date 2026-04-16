using ArchiveFqp.Factories.DisplayDto;
using ArchiveFqp.Factories.DisplayDto.Student;
using ArchiveFqp.Factories.DisplayDto.Teacher;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO;
using ArchiveFqp.Models.DTO.Work;
using ArchiveFqp.Services.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Factories.DisplayDto.Work
{

    public class WorkDtoFactory : IDisplayDtoFactory<WorkDisplayDto, Работа>
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IReferenceDataService _refDataService;
        private readonly StudentDtoFactory _studentDtoFactory;
        private readonly TeacherDtoFactory _teacherDtoFactory;

        private List<ТипРаботы> _workType = [];
        private List<СтатусРаботы> _workStatus = [];
        private List<ДоступРаботы> _workAccess = [];

        private Task _init;

        public WorkDtoFactory(
            IDbContextFactory<ArchiveFqpContext> dbFactory,
            IReferenceDataService refDataService)
        {
            _dbFactory = dbFactory;
            _refDataService = refDataService;
            _studentDtoFactory = new (refDataService);
            _teacherDtoFactory = new(refDataService);

            _init = Task.Run(InitializeLists);
        }

        public WorkDtoFactory(IDbContextFactory<ArchiveFqpContext> dbFactory, 
            IReferenceDataService refDataService, StudentDtoFactory studentDtoFactory, 
            TeacherDtoFactory teacherDtoFactory)
        {
            _dbFactory = dbFactory;
            _refDataService = refDataService;
            _studentDtoFactory = studentDtoFactory;
            _teacherDtoFactory = teacherDtoFactory;

            _init = Task.Run(InitializeLists);
        }

        private async Task InitializeLists()
        {
            _workType = await _refDataService.GetAsync<ТипРаботы>();
            _workStatus = await _refDataService.GetAsync<СтатусРаботы>();
            _workAccess = await _refDataService.GetAsync<ДоступРаботы>();
        }

        public async Task<WorkDisplayDto> CreateDisplayDtoAsync(Работа work)
        {
            _init.Wait();
            return new()
            {
                IdРаботы = work.IdРаботы,
                Тема = work.Тема,
                Студент = await _studentDtoFactory.CreateDisplayDtoAsync(work.IdСтудента) ?? new(),
                Руководитель = await _teacherDtoFactory.CreateDisplayDtoAsync(work.IdПреподавателя, work.IdДолжности),
                ТипРаботы = _workType.FirstOrDefault(o => o.IdТипаРаботы == work.IdТипаРаботы)?.Название ?? "",
                СтатусРаботы = _workStatus.FirstOrDefault(o => o.IdСтатусаРаботы == work.IdСтатусаРаботы)?.Название ?? "",
                ДоступРаботы = _workAccess.FirstOrDefault(o => o.IdДоступаРаботы == work.IdДоступаРаботы)?.Название ?? "",
                Аннотация = work.Аннотация,
                КоличСтраниц = work.КоличСтраниц ?? 0,
                ДатаДобавления = work.ДатаДобавления,
                ДатаИзменения = work.ДатаИзменения,
                Местоположение = work.Местоположение
            };
        }

        public async Task<WorkDisplayDto> CreateDisplayDtoAsync(Работа work, List<Консультант> consultants, List<Рецензент> reviewers)
        {
            _init.Wait();
            return new()
            {
                IdРаботы = work.IdРаботы,
                Тема = work.Тема,
                Студент = (await _studentDtoFactory.CreateDisplayDtoAsync(work.IdСтудента))!,
                Руководитель = await _teacherDtoFactory.CreateDisplayDtoAsync(work.IdПреподавателя, work.IdДолжности),
                Консультанты = await _teacherDtoFactory.CreateDisplayDtoListAsync(consultants),
                Рецензенты = await _teacherDtoFactory.CreateDisplayDtoListAsync(reviewers),
                ТипРаботы = _workType.First(o => o.IdТипаРаботы == work.IdТипаРаботы).Название,
                СтатусРаботы = _workStatus.First(o => o.IdСтатусаРаботы == work.IdСтатусаРаботы).Название,
                ДоступРаботы = _workAccess.First(o => o.IdДоступаРаботы == work.IdДоступаРаботы).Название,
                Аннотация = work.Аннотация,
                КоличСтраниц = work.КоличСтраниц ?? 0,
                ДатаДобавления = work.ДатаДобавления,
                ДатаИзменения = work.ДатаИзменения,
                Местоположение = work.Местоположение
            };
        }

        public async Task<WorkDisplayDto?> CreateDisplayDtoAsync(int id)
        {
            _init.Wait();
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();
            Работа? work = context.Работаs.FirstOrDefault(o => o.IdРаботы == id);
            if (work == null) return null;

            return await CreateDisplayDtoAsync(work);
        }

        public async Task<List<WorkDisplayDto>> CreateDisplayDtoListAsync(IEnumerable<Работа> works)
        {
            IEnumerable<Task<WorkDisplayDto>> tasks = works.Select(CreateDisplayDtoAsync);
            WorkDisplayDto[] results = await Task.WhenAll(tasks);
            return [.. results];
        }
    }
}
