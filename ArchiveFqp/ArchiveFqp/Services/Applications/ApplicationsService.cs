using ArchiveFqp.Factories.DisplayDto.WorkApplication;
using ArchiveFqp.Interfaces;
using ArchiveFqp.Interfaces.Applications;
using ArchiveFqp.Interfaces.Attributes;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.User;
using ArchiveFqp.Interfaces.Work;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Attribute;
using ArchiveFqp.Models.DTO.Work;
using ArchiveFqp.Models.DTO.WorkApplication;
using ArchiveFqp.Models.Settings.SettingsArchive;
using ArchiveFqp.Services.Attributes;
using ArchiveFqp.Services.User;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ArchiveFqp.Services.Applications
{
    public class ApplicationsService : CrudGeneric, IApplicationsService
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IAttributeService _attributeService;
        private readonly IReferenceDataService _refDataService;
        private readonly IUserService _userService;
        private readonly IWorkService _workService;
        private readonly SettingsArchive _settings;

        public ApplicationsService(IDbContextFactory<ArchiveFqpContext> dbFactory, IAttributeService attributeService,
            IReferenceDataService referenceDataService, IUserService userService, IWorkService workService,
            SettingsArchive settings)
        {
            _dbFactory = dbFactory;
            _refDataService = referenceDataService;
            _attributeService = attributeService;
            _userService = userService;
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
            WorkDisplayDto work = await _workService.GetWorkDisplayAsync(app.IdРаботы, consultants, reviewers);
            WorkApplicationDtoFactory factory = new(_dbFactory, _userService, _refDataService);
            return await factory.CreateDisplayDtoAsync(app, work);
        }

        public async Task<bool> AddWorkApplication(WorkApplicationDto workApplication)
        {
            ЗаявлениеРаботы newApp = new()
            {
                IdРаботы = workApplication.IdРаботы!.Value,
                Цель = workApplication.Цель!,
                ДатаПоступления = DateTime.Now,
                ДатаВозврПоЗаявл = workApplication.ДатаВозврПоЗаявл!.Value,
                IdПользователя = workApplication.IdПользователя,
                IdСтатуса = workApplication.IdСтатуса
            };

            await base.Upsert(newApp, _dbFactory);
            workApplication.IdЗаявления = newApp.IdЗаявления;

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
            return await base.Upsert(attributeApplication, _dbFactory);
        }

        public async Task<bool> AddAnswerAttributeApplication(ЗаявлениеАтрибута attributeApplication)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            ЗаявлениеАтрибута? app = context.ЗаявлениеАтрибутаs.Find(attributeApplication.IdЗаявления);
            if (app == null) return false;

            app.Ответ = attributeApplication.Ответ;
            app.ДатаОтвета = DateTime.Now;
            app.IdСтатуса = attributeApplication.IdСтатуса;
            app.НазваниеИтог = attributeApplication.НазваниеИтог;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteAttributeApplication(ЗаявлениеАтрибута attributeApplication, string attributeQuery)
        {
            Атрибут attr = new()
            {
                IdАтрибута = attributeApplication.IdАтрибута ?? 0,
                Название = attributeApplication.Название,
            };

            if (attributeApplication.Новый)
            {
                AttributeSettings settings = new()
                {
                    Query = attributeQuery,
                    Examples = attributeApplication.Примеры,
                    Keywords = attributeApplication.КлючевыеСлова
                };
                attr.Настройки = JsonConvert.SerializeObject(settings, new JsonSerializerSettings
                    {
                        // наименование полей с маленькой буквы
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    });

                await _attributeService.Upsert(attr);
            }


            if (attributeApplication.IdИнститута != null)
            {
                await _attributeService.Upsert(new АтрибутИнститута()
                {
                    IdАтрибута = attr.IdАтрибута,
                    IdСтатусаРаботы = null,
                    IdТипаРаботы = attributeApplication.IdТипаРаботы,
                    IdИнститута = attributeApplication.IdИнститута.Value
                });
            }
            else if (attributeApplication.IdКафедры != null)
            {
                await _attributeService.Upsert(new АтрибутКафедры()
                {
                    IdАтрибута = attr.IdАтрибута,
                    IdСтатусаРаботы = null,
                    IdТипаРаботы = attributeApplication.IdТипаРаботы,
                    IdКафедры = attributeApplication.IdКафедры.Value
                });
            }
            else if (attributeApplication.IdНаправления != null)
            {
                await _attributeService.Upsert(new АтрибутНаправления()
                {
                    IdАтрибута = attr.IdАтрибута,
                    IdСтатусаРаботы = null,
                    IdТипаРаботы = attributeApplication.IdТипаРаботы,
                    IdНаправления = attributeApplication.IdНаправления.Value
                });
            }
            else if (attributeApplication.IdПрофиля != null)
            {
                await _attributeService.Upsert(new АтрибутПрофиля()
                {
                    IdАтрибута = attr.IdАтрибута,
                    IdСтатусаРаботы = null,
                    IdТипаРаботы = attributeApplication.IdТипаРаботы,
                    IdПрофиля = attributeApplication.IdПрофиля.Value
                });
            }
            else
            {
                await _attributeService.Upsert(new АтрибутУчреждения()
                {
                    IdАтрибута = attr.IdАтрибута,
                    IdСтатусаРаботы = null,
                    IdТипаРаботы = attributeApplication.IdТипаРаботы
                });
            }

            return true;
        }

        public async Task<bool> CompleteWorkApplication(WorkApplicationDto workApplication)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            ЗаявлениеРаботы? app = context.ЗаявлениеРаботыs.Find(workApplication.IdЗаявления);
            if (app == null) return false;

            app.IdСтатуса = workApplication.IdСтатуса;
            app.ДатаВозврПоФакту = DateTime.Now;

            await context.SaveChangesAsync();
            return true;
        }
    }
}
