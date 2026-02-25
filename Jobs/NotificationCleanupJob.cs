using MoM.NotificationService.Repositories;
using Quartz;

namespace MoM.NotificationService.Jobs;

public class NotificationCleanupJob : IJob
{
    private readonly NotificationLogRepository _logRepo;
    private readonly ILogger<NotificationCleanupJob> _logger;

    public NotificationCleanupJob(
        NotificationLogRepository logRepo,
        ILogger<NotificationCleanupJob> logger)
    {
        _logRepo = logRepo;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("🧹 Cleanup Job Started at {time}", DateTime.Now);

        var deleted = await _logRepo.DeleteOlderThanAsync(6);

        _logger.LogInformation("🧹 Cleanup Completed. Deleted {count} old records.", deleted);
    }
}