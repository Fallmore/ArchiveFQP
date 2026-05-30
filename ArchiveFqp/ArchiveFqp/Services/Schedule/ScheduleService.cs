using Quartz;
using Quartz.Impl.Matchers;
using ArchiveFqp.Jobs;
using ArchiveFqp.Interfaces.Schedule;

namespace ArchiveFqp.Services.Schedule
{

    public class ScheduleService : IScheduleService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<ScheduleService> _logger;

        public ScheduleService(
            ISchedulerFactory schedulerFactory,
            ILogger<ScheduleService> logger,
            IConfiguration configuration)
        {
            _schedulerFactory = schedulerFactory;
            _logger = logger;
        }

        public async Task UpdateScheduleAsync<T>(string cronExpression, bool enabled) where T : class
        {
            string jobKey = typeof(T).Name;
            string triggerKey = jobKey + "Trigger";

            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKeyObj = new JobKey(jobKey);

            if (!enabled)
            {
                // Отключаем триггер если нужно
                var triggers = await scheduler.GetTriggersOfJob(jobKeyObj);
                foreach (var trigger in triggers)
                {
                    await scheduler.UnscheduleJob(trigger.Key);
                }
                _logger.LogInformation("Автоматический бэкап отключен");
                return;
            }

            // Создаем новый триггер с обновленным расписанием
            var triggerKeyObj = new TriggerKey(triggerKey);
            var newTrigger = TriggerBuilder.Create()
                .WithIdentity(triggerKeyObj)
                .WithCronSchedule(cronExpression, x => x
                    .InTimeZone(TimeZoneInfo.Local)
                    .WithMisfireHandlingInstructionFireAndProceed())
                .Build();

            // Перепланируем задачу
            await scheduler.RescheduleJob(triggerKeyObj, newTrigger);
            _logger.LogInformation("Расписание бэкапа обновлено: {CronExpression}", cronExpression);
        }

        public async Task<string> GetCurrentScheduleAsync<T>() where T : class
        {
            string jobKey = typeof(T).Name;

            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKeyObj = new JobKey(jobKey);
            var triggers = await scheduler.GetTriggersOfJob(jobKeyObj);
            var trigger = triggers.FirstOrDefault();

            if (trigger is ICronTrigger cronTrigger)
            {
                return cronTrigger.CronExpressionString ?? "0 0 2 * * ?";
            }

            return "0 0 2 * * ?"; // Значение по умолчанию
        }

        public async Task<bool> IsJobRunningAsync<T>() where T : class
        {
            string jobKey = typeof(T).Name;
            var scheduler = await _schedulerFactory.GetScheduler();
            var currentlyExecuting = await scheduler.GetCurrentlyExecutingJobs();
            return currentlyExecuting.Any(j => j.JobDetail.Key.Name == jobKey);
        }

        public async Task TriggerNowAsync<T>() where T : class
        {
            string jobKey = typeof(T).Name;
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKeyObj = new JobKey(jobKey);
            await scheduler.TriggerJob(jobKeyObj);
            _logger.LogInformation("Ручной запуск бэкапа выполнен");
        }
    }
}
