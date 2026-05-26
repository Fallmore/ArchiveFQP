using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.Settings;
using Newtonsoft.Json;

namespace ArchiveFqp.Interfaces.Settings
{
    public interface ISettingsService
    {
        public Task<List<T>> GetAllSettings<T>() where T : class;

        /// <summary>
        /// Получает настройки объекта от типа <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>T может быть одним из следующих типов: <see cref="НастройкиУчреждения"/>, <see cref="НастройкиИнститута"/>, <see cref="НастройкиКафедры"/>, <see cref="НастройкиПользователя"/></remarks>
        /// <typeparam name="T">1 из <see cref="НастройкиУчреждения"/>, <see cref="НастройкиИнститута"/>, <see cref="НастройкиКафедры"/>, <see cref="НастройкиПользователя"/></typeparam>
        public Task<T?> GetSettings<T>(int idOwner) where T : class;

        /// <summary>
        /// Конвертирует json настроек в DTO типа <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns><inheritdoc cref="JsonConvert.DeserializeObject{T}(string, JsonSerializerSettings?)"/></returns>
        public Task<T?> GetSettingsJson<T>(string json) where T : BaseSettings;

        /// <summary>
        /// Добавляет DTO настроек типа <typeparamref name="T"/> как json в объект типа <typeparamref name="V"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <returns>True, если тип T относится к настройкам, False - в ином случае</returns>
        public Task<bool> AddSettingsAsJson<T, V>(T settingsDto, V settings) where T : BaseSettings where V : class;


        public Task<bool> Upsert<T>(T settings) where T : class;
        public Task<bool> Delete<T>(int id) where T : class;
    }
}
