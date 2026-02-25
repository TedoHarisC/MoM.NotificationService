using Quartz;

namespace MoM.NotificationService.Jobs;

public class Level2ReminderJob : IJob
{
    private readonly ILogger<Level2ReminderJob> _logger;

    public Level2ReminderJob(ILogger<Level2ReminderJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Level 2 Reminder Job Started at {time}", DateTime.Now);

        try
        {
            // TODO:
            // 1. Ambil semua MoM Level 2 Outstanding
            // 2. Group by Dept
            // 3. Cek NotificationLog
            // 4. Kirim 1 email per dept
            // 5. Insert log

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Level2ReminderJob");
        }

        _logger.LogInformation("Level 2 Reminder Job Finished at {time}", DateTime.Now);
    }
}