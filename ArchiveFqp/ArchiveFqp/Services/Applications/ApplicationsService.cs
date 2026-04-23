using ArchiveFqp.Factories.DisplayDto.WorkApplication;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Work;
using ArchiveFqp.Models.DTO.WorkApplication;
using ArchiveFqp.Models.Settings.SettingsArchive;
using ArchiveFqp.Services.ReferenceData;
using ArchiveFqp.Services.Work;
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
    }
}
