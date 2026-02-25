using MoM.NotificationService.Repositories;
using MoM.NotificationService.Services;
using MoM.NotificationService.Templates;
using Quartz;

namespace MoM.NotificationService.Jobs;

public class Level2ReminderJob : IJob
{
    private readonly NotificationLogRepository _logRepo;
    private readonly EmailService _emailService;
    private readonly ILogger<Level2ReminderJob> _logger;
    private readonly MoMQueryService _momService;

    public Level2ReminderJob(
        ILogger<Level2ReminderJob> logger,
        NotificationLogRepository logRepo,
        EmailService emailService,
        MoMQueryService momService)
    {
        _logger = logger;
        _logRepo = logRepo;
        _emailService = emailService;
        _momService = momService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("🔥 Level 2 Reminder Job Triggered at {time}", DateTime.Now);

        try
        {
            // 1️⃣ Ambil semua Level 2 Outstanding
            var allMoms = await _momService.GetOutstandingLevel2Async();

            if (!allMoms.Any())
            {
                _logger.LogInformation("No outstanding Level 2 MoM found.");
                return;
            }

            // 2️⃣ Group by Dept
            var groupedByDept = allMoms.GroupBy(x => x.Dept);

            foreach (var deptGroup in groupedByDept)
            {
                var dept = deptGroup.Key;
                var moms = deptGroup.ToList();

                // 3️⃣ Anti spam check
                var alreadySent = await _logRepo.ExistsAsync("LEVEL2", dept, DateTime.Today);
                if (alreadySent)
                {
                    _logger.LogInformation("Already sent Level 2 reminder for {dept}", dept);
                    continue;
                }

                if (moms.Count > 200)
                {
                    _logger.LogWarning("Abnormal MoM count detected for {dept}. Skipping send.", dept);
                    continue;
                }

                // 4️⃣ Ambil Dept Head + Sect Head
                var headRecipients = await _momService.GetLevel1RecipientsAsync(dept);

                // 5️⃣ Ambil PIC emails & names
                var picRecipients = new List<string>();

                foreach (var mom in moms)
                {
                    var picEmails = await _momService.GetPICEmailsAsync(mom.MoMId);
                    picRecipients.AddRange(picEmails);

                    var picNames = await _momService.GetPICNamesAsync(mom.MoMId);
                    mom.PICs = picNames;
                }

                // 6️⃣ Gabungkan recipients
                var recipients = headRecipients
                    .Concat(picRecipients)
                    .Distinct()
                    .ToList();

                if (!recipients.Any())
                {
                    _logger.LogWarning("No recipients found for {dept}", dept);
                    continue;
                }

                var subject = $"Reminder MoM Level 2 - Dept {dept} ({moms.Count} Outstanding)";
                var body = Level2EmailTemplate.Generate(dept, moms);

                _logger.LogInformation("Sending Level 2 email to {count} recipients for {dept}",
                    recipients.Count, dept);

                await _emailService.SendAsync(recipients, subject, body);

                await _logRepo.InsertAsync("LEVEL2", dept, DateTime.Today, moms.Count);

                _logger.LogInformation("Level 2 email sent for {dept}", dept);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Level2ReminderJob");
        }
    }
}