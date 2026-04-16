using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.Settings.SettingsArchive;
using ArchiveFqp.Services.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Services.ExpirationCheck
{
    /// <summary>
    /// Сервис проверки истечения срока действия заявлений просмотра работ
    /// </summary>
    public class ExpirationCheckService : BackgroundService
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly ILogger<ReferenceDataService> _logger;
        private readonly IReferenceDataService _refDataService;
        private readonly SettingsArchive _settings;
        private List<ВыдачаРаботы> _workApplications = null!;
        private СтатусВыдачи _workApplicationsActiveStatus = null!;
        private СтатусВыдачи _workApplicationsRejectStatus = null!;
        private СтатусВыдачи _workApplicationsCompleteStatus = null!;
        private List<ВыдачаРаботы> _workApplicationsActive = null!;

        public ExpirationCheckService(IDbContextFactory<ArchiveFqpContext> dbFactory,
            ILogger<ReferenceDataService> logger, IReferenceDataService refDataService,
            SettingsArchive settings)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _refDataService = refDataService;
            _settings = settings;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            // загружаем справочные данные асинхронно при старте
            _workApplications = await _refDataService.GetAsync<ВыдачаРаботы>();
            await GetWorkApplicationsStatusesAsync();
            _workApplicationsActive = UpdateWorkApplicationsActive();

            // подписка на изменения (можно подписываться тут)
            _refDataService.TablesChanged += UpdateWorkApplicationsData;

            _logger.LogInformation("ExpirationCheckService starting");
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Сервис ExpirationCheckService проверки истечения срока действия заявлений на выдачу работ запущен.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckExpiredApplications(stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // корректный выход при остановке хоста
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в ExpirationCheckService во время цикла проверки");
                    // при ошибке подождать коротко чтобы избежать горячего цикла
                    await Task.Delay(TimeSpan.FromSeconds(10), CancellationToken.None);
                }
            }
        }

        /// <summary>
        /// Проверяет истекшие заявления на выдачу работ и обновляет их статусы.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task CheckExpiredApplications(CancellationToken stoppingToken)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            List<ВыдачаРаботы> expiredWorkApplications = _workApplicationsActive
                .Where(a => a.ДатаВозврПоЗаявл <= DateTime.Now)
                .Select(a =>
                {
                    // Если срок действия заявления истек, то статус меняется на "Завершено",
                    // если он был "Активно", и на "Отклонено" во всех остальных случаях
                    if (a.IdСтатусаВыдачи == _workApplicationsActiveStatus.IdСтатусаВыдачи)
                    {
                        a.IdСтатусаВыдачи = _workApplicationsCompleteStatus.IdСтатусаВыдачи;
                    }
                    else
                    {
                        a.IdСтатусаВыдачи = _workApplicationsRejectStatus.IdСтатусаВыдачи;
                        a.ДатаОтвета = DateTime.Now;
                        a.Ответ = "Заявление отклонено системой из-за истечения срока проверки.";
                    }
                    return a;
                })
                .ToList();

            if (expiredWorkApplications.Count != 0)
            {
                _logger.LogDebug("Найдено {Count} истекших заявлений на выдачу работ. Обновление статусов...", expiredWorkApplications.Count);
                context.ВыдачаРаботыs.UpdateRange(expiredWorkApplications);
                await context.SaveChangesAsync(stoppingToken);
            }
        }

        private async void UpdateWorkApplicationsData(object? sender, List<string> e)
        {
            if (e.Contains(typeof(ВыдачаРаботы).Name))
            {
                try
                {
                    _workApplications = await _refDataService.GetAsync<ВыдачаРаботы>();
                    _workApplicationsActive = UpdateWorkApplicationsActive();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка обновления кэша {Name}", typeof(ВыдачаРаботы).Name);
                }
            }
        }

        /// <summary>
        /// Обновляет список активных заявлений на выдачу работ, 
        /// исключая те, которые уже завершены или отклонены
        /// </summary>
        /// <returns></returns>
        private List<ВыдачаРаботы> UpdateWorkApplicationsActive()
        {
            return _workApplications
                .Where(a => a.IdСтатусаВыдачи != _workApplicationsCompleteStatus.IdСтатусаВыдачи &&
                            a.IdСтатусаВыдачи != _workApplicationsRejectStatus.IdСтатусаВыдачи)
                .ToList();
        }

        /// <summary>
        /// Инициализирует внутренние поля, связанные со статусами заявок на работу, на основе настроек приложения.
        /// </summary>
        /// <remarks>Этот метод должен вызываться перед операциями, которые зависят от статусов заявок на
        /// работу. Метод блокирует вызывающий поток до завершения асинхронной операции получения справочных
        /// данных.</remarks>
        private async Task GetWorkApplicationsStatusesAsync()
        {
            List<СтатусВыдачи> statuses = await _refDataService.GetAsync<СтатусВыдачи>();
            _workApplicationsCompleteStatus = statuses
                        .First(a => _settings.WorkApplicationsCompleteStatus == a.Название);

            _workApplicationsActiveStatus = statuses
                .First(a => _settings.WorkApplicationsActiveStatus == a.Название);

            _workApplicationsRejectStatus = statuses
                .First(a => _settings.WorkApplicationsRejectStatus == a.Название);
        }

        public override void Dispose()
        {
            _refDataService.TablesChanged -= UpdateWorkApplicationsData;
            base.Dispose();
        }
    }
}
