namespace ArchiveFqp.Interfaces.Schedule
{
    public interface IScheduleService
    {
        Task UpdateScheduleAsync<T>(string cronExpression, bool enabled) where T : class;
        Task<string> GetCurrentScheduleAsync<T>() where T : class;
        Task<bool> IsJobRunningAsync<T>() where T : class;
        public Task TriggerNowAsync<T>() where T : class;
    }
}
