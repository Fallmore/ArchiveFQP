using ArchiveFqp.Factories.DisplayDto.WorkApplication;
using ArchiveFqp.Interfaces.Applications;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.Work;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Work;
using ArchiveFqp.Models.DTO.WorkApplication;
using ArchiveFqp.Models.Settings.SettingsArchive;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Services.Applications
{
    public class ApplicationsService : IApplicationsService
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IReferenceDataService _refDataService;
        private readonly IWorkService _workService;
        private readonly SettingsArchive _settings;

        public ApplicationsService(IDbContextFactory<ArchiveFqpContext> dbFactory,
            IReferenceDataService referenceDataService, IWorkService workService,
            SettingsArchive settings)
        {
            _dbFactory = dbFactory;
            _refDataService = referenceDataService;
            _workService = workService;
            _settings = settings;
        }

        public async Task<List<СтатусЗаявления>> GetApplicationsStatusesAsync()
        {
            return await _refDataService.GetAsync<СтатусЗаявления>();
        }

        public async Task<List<ЗаявлениеАтрибута>> GetAttributeApplicationsAsync()
        {
            return await _refDataService.GetAsync<ЗаявлениеАтрибута>();
        }

        public async Task<List<ЗаявлениеРаботы>> GetWorkApplicationsAsync()
        {
            return await _refDataService.GetAsync<ЗаявлениеРаботы>();
        }

        public async Task<WorkApplicationDto> GetWorkApplicationAsync(ЗаявлениеРаботы app, List<Консультант>? consultants = null, List<Рецензент>? reviewers = null)
        {
            WorkDisplayDto work = await _workService.GetWorkDisplayAsync(app.IdРаботы);
            WorkApplicationDtoFactory factory = new(_dbFactory, _refDataService);
            return await factory.CreateDisplayDtoAsync(app, work);
        }

        public async Task<bool> AddWorkApplication(WorkApplicationDto workApplication)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            ЗаявлениеРаботы newApp = new()
            {
                IdРаботы = workApplication.IdРаботы!.Value,
                Цель = workApplication.Цель!,
                ДатаПоступления = DateTime.Now,
                ДатаВозврПоЗаявл = workApplication.ДатаВозврПоЗаявл!.Value,
                IdПользователя = workApplication.IdПользователя,
                IdСтатуса = workApplication.IdСтатуса
            };

            context.ЗаявлениеРаботыs.Add(newApp);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddAnswerWorkApplication(WorkApplicationDto workApplication)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            ЗаявлениеРаботы? app = context.ЗаявлениеРаботыs.Find(workApplication.IdЗаявления);
            if (app == null) return false;

            app.Ответ = workApplication.Ответ;
            app.ДатаОтвета = DateTime.Now;
            app.IdСтатуса = workApplication.IdСтатуса;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddAttributeApplication(ЗаявлениеАтрибута attributeApplication)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();
            ЗаявлениеАтрибута newApp = new()
            {
                IdАтрибута = attributeApplication.IdАтрибута,
                Описание = attributeApplication.Описание!,
                ДатаПоступления = DateTime.Now,
                IdИнститута = attributeApplication.IdИнститута,
                IdКафедры = attributeApplication.IdКафедры,
                IdНаправления = attributeApplication.IdНаправления,
                IdПрофиля = attributeApplication.IdПрофиля,
                Название = attributeApplication.Название,
                Новый = attributeApplication.Новый,
                Примеры = attributeApplication.Новый ? attributeApplication.Примеры : null,
                IdПользователя = attributeApplication.IdПользователя,
                IdСтатуса = attributeApplication.IdСтатуса
            };

            context.ЗаявлениеАтрибутаs.Add(newApp);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddAnswerAttributeApplication(ЗаявлениеАтрибута attributeApplication)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            ЗаявлениеАтрибута? app = context.ЗаявлениеАтрибутаs.Find(attributeApplication.IdЗаявления);
            if (app == null) return false;

            app.Ответ = attributeApplication.Ответ;
            app.ДатаОтвета = DateTime.Now;
            app.IdСтатуса = attributeApplication.IdСтатуса;

            await context.SaveChangesAsync();
            return true;
        }
    }
}
