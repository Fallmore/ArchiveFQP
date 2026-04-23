using ArchiveFqp.Factories.DisplayDto.User;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Attribute;
using ArchiveFqp.Models.DTO.User;
using ArchiveFqp.Services.DatabaseNotification;
using ArchiveFqp.Services.Work;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ArchiveFqp.Services.ReferenceData
{

    /// <summary>
    /// Сервис взаимодействия с данными таблиц БД через кэш
    /// </summary>
    public partial class ReferenceDataService : BackgroundService, IReferenceDataService, IDisposable
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IMemoryCache _cache;
        private readonly IDatabaseNotificationService _notificationService;
        private readonly ILogger<ReferenceDataService> _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
        private static readonly string s_cacheAllDataName = "all_reference_data";
        private static readonly string s_cacheAttributeValues = "ref_dict_attribute_values";
        private static readonly string s_cacheUserAccounts = "ref_user_account_values";

        public event EventHandler<List<string>>? TablesChanged;

        protected virtual void OnTablesChanged(List<string> e)
        {
            TablesChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Таблицы, входящие в общий кэш. <br/> Названия таблиц имеют стиль snake_case
        /// </summary>
        public List<string> TablesInSnapshot = GetTablesName(ReferenceDataSnapshot.GetStaticTableNames());

        /// <summary>
        /// Таблицы, не входящие в общий кэш. <br/> Названия таблиц имеют стиль snake_case
        /// </summary>
        public List<string> TablesSeparated { get; set; } = ["пользователь", "заявление_работы",
            "статус_заявления", "работа", "роль_пользователя", "аккаунт_пользователя", "заявление_атрибута"];

        /// <summary>
        /// Таблицы, от которых наследуются другие таблицы
        /// </summary>
        public List<string> TablesInherited { get; set; } = ["консультант", "атрибут_учреждения"];

        /// <summary>
        /// Таблицы, обновления которых нужно следить для обновления всех возможных значений атрибутов
        /// </summary>
        public List<string> TablesAttributeValues { get; set; } = ["данные_по_атриб"];

        public ReferenceDataService(
            IDbContextFactory<ArchiveFqpContext> dbFactory, IMemoryCache memoryCache,
            IDatabaseNotificationService notificationService, ILogger<ReferenceDataService> logger)
        {
            _dbFactory = dbFactory;
            _cache = memoryCache;
            _notificationService = notificationService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await SubscribeToTableChanges();
            await UpdateAttributeValuesAsync();
            await RefreshSnapshotAsync();
            await UpdateUserAccountAsync();
        }

        private async Task SubscribeToTableChanges()
        {
            List<string> tables = [.. TablesInSnapshot];
            tables = tables
                .Union(TablesSeparated)
                .Union(TablesInherited)
                .Union(TablesAttributeValues).ToList();

            await SubscribeToTableAsync(tables);
        }

        private async Task SubscribeToTableAsync(List<string> tableNames)
        {
            try
            {
                await _notificationService.SubscribeAsync(tableNames, async (changeEvent) =>
                {
                    _logger.LogInformation("[{Time}] Таблица {TableName} изменена: {ChangeType}",
                        DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), changeEvent.TableName, changeEvent.ChangeType);

                    if (GetTableClassName(changeEvent.TableName) == typeof(АккаунтПользователя).Name)
                    {
                        await UpdateUserAccountAsync();
                        OnTablesChanged([GetTableClassName(changeEvent.TableName)]);
                        return;
                    }

                    // Определяем класс таблицы по имени и вызываем обновление этой таблицы
                    Type type = Type.GetType(GetTableClassFullName(changeEvent.TableName))!;
                    MethodInfo? method = typeof(ReferenceDataService).GetMethod(nameof(UpdateSingleTableAsync));
                    MethodInfo? genericMethod = method?.MakeGenericMethod(type);
                    await (Task)genericMethod?.Invoke(this, [false, false])!;

                    if (TablesAttributeValues.Contains(changeEvent.TableName))
                    {
                        await UpdateAttributeValuesAsync();
                    }
                    else if (GetTableClassName(changeEvent.TableName) == typeof(Пользователь).Name)
                    {
                        await UpdateUserAccountAsync();
                    }

                    OnTablesChanged([GetTableClassName(changeEvent.TableName)]);

                    // В итоге обновляем всю таблицу, а не измененную запись.
                    // Если обновление всей таблицы будет дорогой операцией для вас,
                    // то как раз используйте TableChangeEvent.
                    // Если впервые с этим связываетесь, то рекомендую у ВСЕХ классов таблиц
                    // добавить интерфейс, содержащий метод GetId(), чтобы успешно находить
                    // в таблице измененную запись и перезаписать её.
                    // Что-то типа такого collection.FirstOrDefault(x => x.GetId().Equals(newItem.GetId()))
                    // Я этого делать не буду, потому что не имею на это столько времени :<
                    // UPD: вроде должно работать и обычное collection.Find, т.к. Old всегда есть
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка подписки на таблицы");
            }
        }

        public void ClearCache()
        {
            _cache.Remove(s_cacheAllDataName);
            foreach (string t in TablesSeparated)
                _cache.Remove(t);
            _logger.LogInformation("Очищен кэш данных всех таблиц");
        }

        /// <summary>
        /// CamelCase в snake_case
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetTableName<T>()
        {
            return UpperSymbols().Replace(typeof(T).Name, (x) => x.Index == 0 ? x.Value.ToLower() : $"_{x.Value.ToLower()}");
        }

        [GeneratedRegex(@"[A-Z]|[А-Я]")]
        private static partial Regex UpperSymbols();

        /// <summary>
        /// CamelCase в snake_case
        /// </summary>
        /// <param name="tableNames"></param>
        /// <returns></returns>
        public static List<string> GetTablesName(List<string> tableNames)
        {
            return tableNames
                .Select(n => UpperSymbols().Replace(n, m => m.Index == 0 ? m.Value.ToLower() : $"_{m.Value.ToLower()}"))
                .ToList();
        }

        /// <summary>
        /// snake_case в CamelCase
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetTableClassName(string name)
        {
            return string.Concat(name.Split('_')
                .Select(x => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x)));
        }

        /// <summary>
        /// snake_case в CamelCase вместе с namespace класса
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetTableClassFullName(string name)
        {
            // Берём любой класс таблиц для того, чтобы получить их общий namespace
            string classNameSpace = (typeof(Атрибут).FullName)!;
            classNameSpace = classNameSpace[..(classNameSpace.LastIndexOf('.') + 1)];
            return string.Concat(classNameSpace, GetTableClassName(name));
        }

        private static string GetCacheKey(string tableName)
        {
            return $"ref_{tableName}";
        }

        private async Task<List<T>> LoadTableData<T>(ArchiveFqpContext context, bool onlyParentData = true) where T : class
        {
            if (TablesInherited.Contains(GetTableName<T>()) && onlyParentData)
            {
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
                return await context.Set<T>()
                    .FromSqlRaw($"SELECT * FROM ONLY {GetTableName<T>()}")
                    .ToListAsync();
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.
            }

            return await context.Set<T>().ToListAsync();
        }

        /// <summary>
        /// <inheritdoc cref="IReferenceDataService.GetAsync{T}(bool, bool)"/>
        /// </summary>
        /// <remarks>Также поддерживает такие DTO объекты как <see cref="AttributeValuesDto"/>
        /// и <see cref="UserDisplayDto"/> (вместо <see cref="АккаунтПользователя"/>)</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="forceRefresh"></param>
        /// <param name="onlyParentData"></param>
        /// <returns><inheritdoc cref="IReferenceDataService.GetAsync{T}(bool, bool)"/></returns>
        public async Task<List<T>> GetAsync<T>(bool forceRefresh, bool onlyParentData) where T : class
        {
            string tableName = GetTableName<T>();
            string cacheKey;

            if (TablesInSnapshot.Contains(tableName))
            {
                ReferenceDataSnapshot snapshot = await GetAllAsync(forceRefresh);
                return snapshot.GetTable<T>();
            }
            else if (typeof(T).Name == typeof(AttributeValuesDto).Name)
            {
                if (_cache.TryGetValue(s_cacheAttributeValues, out List<T>? av))
                {
                    return av!;
                }
                await UpdateAttributeValuesAsync();
                return _cache.Get<List<T>>(s_cacheAttributeValues)!;
            }

            cacheKey = GetCacheKey(tableName);

            if (!forceRefresh && _cache.TryGetValue(cacheKey, out List<T>? cachedData))
            {
                return cachedData!;
            }

            await UpdateSingleTableAsync<T>(forceRefresh, onlyParentData);
            return _cache.Get<List<T>>(cacheKey) ?? [];
        }

        /// <summary>
        /// <inheritdoc cref="IReferenceDataService.GetAllAsync(bool)"/>
        /// </summary>
        /// <param name="forceRefresh"></param>
        /// <returns></returns>
        public async Task<ReferenceDataSnapshot> GetAllAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && _cache.TryGetValue(s_cacheAllDataName, out ReferenceDataSnapshot? snapshot))
            {
                return snapshot!;
            }

            await RefreshSnapshotAsync();
            return _cache.Get<ReferenceDataSnapshot>(s_cacheAllDataName)!;
        }

        /// <summary>
        /// Обновляет кэш с данными объекта типа <see cref="ReferenceDataSnapshot"/>
        /// </summary>
        /// <returns></returns>
        private async Task RefreshSnapshotAsync()
        {
            SemaphoreSlim semaphore = _locks.GetOrAdd(s_cacheAllDataName, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();
            try
            {
                // Double-check после получения блокировки
                if (_cache.TryGetValue(s_cacheAllDataName, out ReferenceDataSnapshot? snapshot))
                {
                    return;
                }

                _logger.LogInformation("[{Time}] Обновление всех данных из БД в кэш",
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));

                using ArchiveFqpContext context = _dbFactory.CreateDbContext();

                snapshot = new ReferenceDataSnapshot
                {
                    Attributes = await context.Атрибутs.ToListAsync(),
                    AttributesOrganization = await context.АтрибутУчрежденияs.ToListAsync(),
                    AttributesInstitute = await context.АтрибутИнститутаs.ToListAsync(),
                    AttributesDepartment = await context.АтрибутКафедрыs.ToListAsync(),
                    AttributesDirection = await context.АтрибутНаправленияs.ToListAsync(),
                    AttributesProfile = await context.АтрибутПрофиляs.ToListAsync(),
                    Posts = await context.Должностьs.ToListAsync(),
                    WorkAccess = await context.ДоступРаботыs.ToListAsync(),
                    Institutes = await context.Институтs.ToListAsync(),
                    Departments = await context.Кафедраs.ToListAsync(),
                    Consultants = await context.Консультантs.FromSqlRaw("SELECT * FROM ONLY \"консультант\"").ToListAsync(),
                    Reviewers = await context.Рецензентs.FromSqlRaw("SELECT * FROM ONLY \"рецензент\"").ToListAsync(),
                    Directions = await context.Направлениеs.ToListAsync(),
                    Teachers = await context.Преподавательs.ToListAsync(),
                    Profiles = await context.Профильs.ToListAsync(),
                    RoleUsers = await context.РольПользователяs.ToListAsync(),
                    WorkStatuses = await context.СтатусРаботыs.ToListAsync(),
                    Students = await context.Студентs.ToListAsync(),
                    WorkTypes = await context.ТипРаботыs.ToListAsync(),
                    Ugsns = await context.Угснs.ToListAsync(),
                    UgsnStandarts = await context.УгснСтандартs.ToListAsync(),
                    EducationLevels = await context.УровеньОбразованияs.ToListAsync(),
                    EducationForms = await context.ФормаОбученияs.ToListAsync(),
                    LastUpdated = DateTime.Now
                };

                _cache.Set(s_cacheAllDataName, snapshot, new MemoryCacheEntryOptions
                {
                    Priority = CacheItemPriority.NeverRemove
                });
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Обновляет кэш с данными одиночных таблиц
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="forceFullRefresh"></param>
        /// <param name="onlyParentData"></param>
        /// <returns></returns>
        public async Task UpdateSingleTableAsync<T>(bool forceFullRefresh = false, bool onlyParentData = false) where T : class
        {
            string tableName = GetTableName<T>();

            if (TablesInSnapshot.Contains(tableName))
            {
                await UpdateSingleTableSnapshotAsync<T>(forceFullRefresh);
                return;
            }

            string cacheKey = GetCacheKey(tableName);
            SemaphoreSlim semaphore = _locks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();
            try
            {
                _logger.LogInformation("[{Time}] Загрузка данных {TableName} из БД",
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), tableName);

                using ArchiveFqpContext? context = _dbFactory.CreateDbContext();
                List<T> data = await LoadTableData<T>(context, onlyParentData);

                _cache.Set(cacheKey, data, new MemoryCacheEntryOptions
                {
                    Priority = CacheItemPriority.NeverRemove
                });
                _logger.LogInformation("[{Time}] Обновлены данные таблицы: {TableName}",
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), tableName);
            }
            finally
            {
                semaphore.Release();
            }

        }


        /// <summary>
        /// Обновляет кэш с данными объекта типа <see cref="ReferenceDataSnapshot"/>, обновляя 1 таблицу
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="forceFullRefresh"></param>
        /// <returns></returns>
        private async Task UpdateSingleTableSnapshotAsync<T>(bool forceFullRefresh = false) where T : class
        {
            string tableName = GetTableName<T>();
            SemaphoreSlim semaphore;
            string cacheKey = s_cacheAllDataName;

            if (forceFullRefresh)
            {
                await RefreshSnapshotAsync();
                return;
            }

            if (!_cache.TryGetValue(cacheKey, out ReferenceDataSnapshot? snapshot) || snapshot == null)
            {
                await RefreshSnapshotAsync();
                return;
            }

            semaphore = _locks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
                using ArchiveFqpContext context = _dbFactory.CreateDbContext();

                snapshot.SetTable(await LoadTableData<T>(context));
                snapshot.LastUpdated = DateTime.Now;

                _cache.Set(cacheKey, snapshot, new MemoryCacheEntryOptions
                {
                    Priority = CacheItemPriority.NeverRemove
                });
                _logger.LogInformation("[{Time}] Обновлены данные таблицы общего кэша: {TableName}",
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), tableName);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Обновляет в кэше справочник данных всех атрибутов в качестве <see cref="AttributeValuesDto"/>
        /// </summary>
        /// <returns></returns>
        private async Task UpdateAttributeValuesAsync()
        {
            SemaphoreSlim semaphore = _locks.GetOrAdd(s_cacheAttributeValues, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
                using ArchiveFqpContext context = _dbFactory.CreateDbContext();

                var query = from data in (await GetAsync<ДанныеПоАтриб>(false, false))
                            join attrStruct in (await GetAsync<АтрибутУчреждения>(false, false))
                                on data.IdСтруктуры equals attrStruct.IdСтруктуры
                            select new { attrStruct.IdАтрибута, data.Данные };

                List<AttributeValuesDto> attributeValues = query
                    .GroupBy(x => x.IdАтрибута)
                    .Select(g => new AttributeValuesDto
                    {
                        IdАтрибута = g.Key,
                        Данные = g.Select(x => x.Данные).Distinct().ToList()
                    }).ToList();

                _cache.Set(s_cacheAttributeValues, attributeValues, new MemoryCacheEntryOptions
                {
                    Priority = CacheItemPriority.NeverRemove
                });

                _logger.LogInformation("[{Time}] Обновлены данные кэша атрибута-значений",
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Обновляет в кэше безопасные данные (без логина и пароля) от 
        /// акаунта пользователя в качестве <see cref="UserDisplayDto"/>
        /// </summary>
        /// <returns></returns>
        private async Task UpdateUserAccountAsync()
        {
            SemaphoreSlim semaphore = _locks.GetOrAdd(s_cacheUserAccounts, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
                UserDtoFactory factory = new(this);

                List<Пользователь> list = await GetAsync<Пользователь>(false, false);
                List<UserDisplayDto> result = await factory.CreateDisplayDtoListAsync(list);

                _cache.Set(s_cacheUserAccounts, result, new MemoryCacheEntryOptions
                {
                    Priority = CacheItemPriority.NeverRemove
                });

                _logger.LogInformation("[{Time}] Обновлены данные кэша акаунта_пользователя",
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
            }
            finally
            {
                semaphore.Release();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _logger.LogInformation("ReferenceDataService disposed");
        }
    }
}
