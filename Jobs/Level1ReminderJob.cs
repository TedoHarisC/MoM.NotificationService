using MoM.NotificationService.Repositories;
using MoM.NotificationService.Services;
using MoM.NotificationService.Templates;
using Quartz;

namespace MoM.NotificationService.Jobs;

public class Level1ReminderJob : IJob
{
    private readonly NotificationLogRepository _logRepo;
    private readonly EmailService _emailService;
    private readonly ILogger<Level1ReminderJob> _logger;
    private readonly MoMQueryService _momService;

    public Level1ReminderJob(
        ILogger<Level1ReminderJob> logger,
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
        _logger.LogInformation("🔥 Level 1 Reminder Job Triggered at {time}", DateTime.Now);

        try
        {
            var depts = GetDeptForToday();

            if (!depts.Any())
            {
                _logger.LogInformation("No department scheduled today.");
                return;
            }

            foreach (var dept in depts)
            {
                var alreadySent = await _logRepo.ExistsAsync("LEVEL1", dept, DateTime.Today);

                if (alreadySent)
                {
                    _logger.LogInformation("Already sent for {dept}", dept);
                    continue;
                }

                var moms = await _momService.GetOutstandingLevel1Async(dept);

                if (!moms.Any())
                {
                    _logger.LogInformation("No outstanding MoM for {dept}", dept);
                    continue;
                }

                if (moms.Count > 200)
                {
                    _logger.LogWarning("Abnormal MoM count detected for {dept}. Skipping send.", dept);
                    continue;
                }

                // Ambil Dept Head (TO)
                var deptHeadEmails = await _momService.GetDeptHeadEmailsAsync(dept);

                // Ambil Sect Head (CC)
                var sectHeadEmails = await _momService.GetSectHeadEmailsAsync(dept);

                // CC → GM
                var gmEmails = await _momService.GetGMEmailsAsync();

                if (!deptHeadEmails.Any())
                {
                    _logger.LogWarning("No Dept Head found for {dept}", dept);
                    continue;
                }

                var toRecipients = deptHeadEmails;

                var ccRecipients = sectHeadEmails
                    .Concat(gmEmails)
                    .Distinct()
                    .Except(toRecipients)
                    .ToList();

                var totalOutstanding = moms.Count;

                var overdueCount = moms.Count(x =>
                    x.DueDate1.HasValue &&
                    x.DueDate1.Value.Date < DateTime.Today);

                string severityIcon;

                if (overdueCount > 0)
                    severityIcon = "🔴";
                else if (totalOutstanding > 0)
                    severityIcon = "🟡";
                else
                    severityIcon = "🟢";

                var subject = $"{severityIcon} Reminder MoM Level 1 - Dept {dept} ({moms.Count} Outstanding)";
                var body = Level1EmailTemplate.Generate(dept, moms, totalOutstanding, overdueCount);

                _logger.LogInformation(
                    "Sending Level 1 email (TO: {toCount}, CC: {ccCount}) for {dept}",
                    toRecipients.Count,
                    ccRecipients.Count,
                    dept);

                await _emailService.SendAsync(toRecipients, ccRecipients, subject, body);

                await _logRepo.InsertAsync("LEVEL1", dept, DateTime.Today, moms.Count);

                _logger.LogInformation("Email sent for {dept}", dept);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Level1ReminderJob");
        }
    }

    // PROD
    private List<string> GetDeptForToday()
    {
        // PROD
        var meetingDay = DateTime.Today.AddDays(2).DayOfWeek;

        return meetingDay switch
        {
            DayOfWeek.Monday => new() { "CPPQA" },
            DayOfWeek.Tuesday => new() { "OPR", "EHS" },
            DayOfWeek.Wednesday => new() { "FA", "SM" },
            DayOfWeek.Thursday => new() { "ENG" },
            DayOfWeek.Friday => new() { "CSR", "HCGS" },
            _ => new()
        };
    }

    // Untuk uji coba mengarah ke Dept
    // private List<string> GetDeptForToday()
    // {
    //     return new() { "HCGS" };
    // }
}