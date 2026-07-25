namespace MoM.NotificationService.Configurations;

public class ScheduleSettings
{
    public string Level1Cron { get; set; } = string.Empty;
    public string Level2Cron { get; set; } = string.Empty;
    public string Level2PersonalizedCron { get; set; } = string.Empty;
    public string TestCron { get; set; } = string.Empty;
    public string CleanupCron { get; set; } = string.Empty;
}