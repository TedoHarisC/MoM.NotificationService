using MoM.NotificationService.Jobs;
using MoM.NotificationService.Repositories;
using MoM.NotificationService.Services;
using Quartz;

namespace MoM.NotificationService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddQuartz(q =>
        {
            var level1JobKey = new JobKey("Level1Job");

            q.AddJob<Level1ReminderJob>(opts => opts.WithIdentity(level1JobKey));

            q.AddTrigger(opts => opts
                .ForJob(level1JobKey)
                .WithIdentity("Level1Trigger")
                //.WithCronSchedule("0 0 18 ? * MON-FRI")); // Production
                .WithCronSchedule("0 * * ? * *")); // Test: tiap menit

            var level2JobKey = new JobKey("Level2Job");

            q.AddJob<Level2ReminderJob>(opts => opts.WithIdentity(level2JobKey));

            q.AddTrigger(opts => opts
                .ForJob(level2JobKey)
                .WithIdentity("Level2Trigger")
                .WithCronSchedule("0 0 21 ? * SUN"));
        });

        builder.Services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        builder.Services.AddSingleton<NotificationLogRepository>();
        builder.Services.AddSingleton<EmailService>();
        builder.Services.AddSingleton<MoMQueryService>();

        var host = builder.Build();
        host.Run();
    }
}