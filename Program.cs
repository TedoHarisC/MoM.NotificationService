using MoM.NotificationService.Configurations;
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
            //! GET SCHEDULE SETTING FROM APPSETTINGS.JSON
            var scheduleSettings = builder.Configuration
                .GetSection("ScheduleSettings")
                .Get<ScheduleSettings>();

            //! JOB : MOM LEVEL 1
            var level1JobKey = new JobKey("Level1Job");

            q.AddJob<Level1ReminderJob>(opts => opts.WithIdentity(level1JobKey));

            q.AddTrigger(opts => opts
                .ForJob(level1JobKey)
                .WithIdentity("Level1Trigger")
                .WithCronSchedule(scheduleSettings!.Level1Cron));
            //.WithCronSchedule("0 0 18 ? * MON-FRI"));
            //.WithCronSchedule("0 * * ? * *")); // Test: tiap menit (TRIAL)

            //! JOB : MOM LEVEL 2
            var level2JobKey = new JobKey("Level2Job");

            q.AddJob<Level2ReminderJob>(opts => opts.WithIdentity(level2JobKey));

            q.AddTrigger(opts => opts
                .ForJob(level2JobKey)
                .WithIdentity("Level2Trigger")
                .WithCronSchedule(scheduleSettings!.TestCron));
            //.WithCronSchedule("0 0 21 ? * SUN"));
            //.WithCronSchedule("0/10 * * ? * *")); // Test: tiap 10 detik (TRIAL)

            //! JOB untuk menghapus Log yang sudah bernilai 6 bulan ke belakang lebih
            var cleanupJobKey = new JobKey("CleanupJob");

            q.AddJob<NotificationCleanupJob>(opts => opts.WithIdentity(cleanupJobKey));

            q.AddTrigger(opts => opts
                .ForJob(cleanupJobKey)
                .WithIdentity("CleanupTrigger")
                .WithCronSchedule(scheduleSettings!.CleanupCron));
            //.WithCronSchedule("0 0 2 1 * ?")); // setiap tanggal 1 jam 02:00 (TRIAL)

        });

        builder.Services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        builder.Services.AddSingleton<NotificationLogRepository>();
        builder.Services.AddSingleton<EmailService>();
        builder.Services.AddSingleton<MoMQueryService>();
        builder.Services.Configure<ScheduleSettings>(
        builder.Configuration.GetSection("ScheduleSettings"));
        builder.Services.AddSingleton<NotificationExecutionLogRepository>();

        var host = builder.Build();
        host.Run();
    }
}