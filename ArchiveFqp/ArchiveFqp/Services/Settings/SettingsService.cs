using ArchiveFqp.Interfaces;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.Settings;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.Settings;
using ArchiveFqp.Models.Settings.Department;
using ArchiveFqp.Models.Settings.Direction;
using ArchiveFqp.Models.Settings.Institute;
using ArchiveFqp.Models.Settings.Profile;
using ArchiveFqp.Models.Settings.SettingsArchive;
using ArchiveFqp.Models.Settings.User;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ArchiveFqp.Services.Settings
{

    public class SettingsService : CrudGeneric, ISettingsService
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IReferenceDataService _refDataService;
        private readonly JsonSerializerSettings _options;

        public SettingsService(IDbContextFactory<ArchiveFqpContext> dbFactory,
            IReferenceDataService refDataService)
        {
            _dbFactory = dbFactory;
            _refDataService = refDataService;
            _options = new()
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
        }

        public async Task<List<T>> GetAllSettings<T>() where T : class
        {
            return await _refDataService.GetAsync<T>();
        }

        public async Task<T?> GetSettings<T>(int idOwner) where T : class
        {
            List<T> settings = await _refDataService.GetAsync<T>();
            return typeof(T).Name switch
            {
                nameof(НастройкиУчреждения) => (settings as List<НастройкиУчреждения>)!.FirstOrDefault() as T,
                nameof(НастройкиИнститута) => (settings as List<НастройкиИнститута>)!.FirstOrDefault(x => x.IdИнститута == idOwner) as T,
                nameof(НастройкиКафедры) => (settings as List<НастройкиКафедры>)!.FirstOrDefault(x => x.IdКафедры == idOwner) as T,
                nameof(НастройкиПользователя) => (settings as List<НастройкиПользователя>)!.FirstOrDefault(x => x.IdПользователя == idOwner) as T,
                _ => null
            };
        }

        public async Task<T?> GetSettingsJson<T>(string json) where T : BaseSettings
        {
            return JsonConvert.DeserializeObject<T>(json, _options);
        }

        public async Task<bool> AddSettingsAsJson<T,V>(T settingsDto, V settings) where T : BaseSettings where V : class
        {
            switch (typeof(T).Name)
            {
                case nameof(SettingsArchive):
                    (settings as НастройкиУчреждения)!.Настройки = JsonConvert.SerializeObject(settingsDto, _options);
                    break;
                case nameof(SettingsInstitute):
                    (settings as НастройкиИнститута)!.Настройки = JsonConvert.SerializeObject(settingsDto, _options);
                    break;
                case nameof(SettingsDepartment):
                    (settings as НастройкиКафедры)!.Настройки = JsonConvert.SerializeObject(settingsDto, _options);
                    break;
                case nameof(SettingsUser):
                    (settings as НастройкиПользователя)!.Настройки = JsonConvert.SerializeObject(settingsDto, _options);
                    break;
                default:
                    return false;
            }

            return true;
        }

        public async Task<bool> Upsert<T>(T settings) where T : class
        {
            return await base.Upsert<T>(settings, _dbFactory);
        }

        public async Task<bool> Delete<T>(int id) where T : class
        {
            return await base.Delete<T>(id, _dbFactory);
        }
    }
}
