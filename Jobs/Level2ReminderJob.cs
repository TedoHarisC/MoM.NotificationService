using MoM.NotificationService.Dto;
using MoM.NotificationService.Repositories;
using MoM.NotificationService.Services;
using MoM.NotificationService.Templates;
using Quartz;

namespace MoM.NotificationService.Jobs;

[DisallowConcurrentExecution]
public class Level2ReminderJob : IJob
{
    private readonly NotificationLogRepository _logRepo;
    private readonly EmailService _emailService;
    private readonly ILogger<Level2ReminderJob> _logger;
    private readonly MoMQueryService _momService;
    private readonly NotificationExecutionLogRepository _executionLogRepo;
    private readonly IConfiguration _configuration;

    public Level2ReminderJob(
        ILogger<Level2ReminderJob> logger,
        NotificationLogRepository logRepo,
        EmailService emailService,
        MoMQueryService momService,
        NotificationExecutionLogRepository executionLogRepo,
        IConfiguration configuration)
    {
        _logger = logger;
        _logRepo = logRepo;
        _emailService = emailService;
        _momService = momService;
        _executionLogRepo = executionLogRepo;
        _configuration = configuration;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("🔥 Level 2 Reminder Job Triggered at {time}", DateTime.Now);

        var startTime = DateTime.Now;
        int executionId = await _executionLogRepo.InsertStartAsync("LEVEL2", startTime);

        int totalEmailSent = 0;
        bool isSuccess = false;
        string? errorMessage = null;

        try
        {
            var alwaysCcNiks = _configuration.GetSection("NotificationSettings:AlwaysCcNiks").Get<List<string>>() ?? new();
            var alwaysCcEmails = await _momService.GetEmailsByNiksAsync(alwaysCcNiks);

            // === DAILY (ForumId = 2): kirim setiap 3 hari dari IssuedDate ===
            var dailyMoms = await _momService.GetOutstandingLevel2DailyAsync();
            totalEmailSent += await SendLevel2EmailsAsync(dailyMoms, "LEVEL2_DAILY", "Daily", alwaysCcEmails);

            // === WEEKLY (ForumId = 3): kirim jika IssuedDate >= 5 hari lalu ===
            var weeklyMoms = await _momService.GetOutstandingLevel2WeeklyAsync();
            totalEmailSent += await SendLevel2EmailsAsync(weeklyMoms, "LEVEL2_WEEKLY", "Weekly", alwaysCcEmails);

            if (!dailyMoms.Any() && !weeklyMoms.Any())
                _logger.LogInformation("No outstanding Level 2 MoM found.");

            isSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Level2ReminderJob");
            isSuccess = false;
            errorMessage = ex.Message;
        }
        finally
        {
            await _executionLogRepo.UpdateEndAsync(
                executionId,
                DateTime.Now,
                isSuccess,
                totalEmailSent,
                errorMessage);
        }
    }

    private async Task<int> SendLevel2EmailsAsync(
        List<MoMLevel2Dto> allMoms,
        string notificationType,
        string typeLabel,
        List<string> alwaysCcEmails)
    {
        int emailSent = 0;

        if (!allMoms.Any()) return emailSent;

        var groupedByDept = allMoms.GroupBy(x => x.Dept);

        foreach (var deptGroup in groupedByDept)
        {
            var dept = deptGroup.Key;
            var moms = deptGroup.ToList();

            var alreadySent = await _logRepo.ExistsAsync(notificationType, dept, DateTime.Today);
            if (alreadySent)
            {
                _logger.LogInformation("Already sent {type} reminder for {dept}", notificationType, dept);
                continue;
            }

            if (moms.Count > 200)
            {
                _logger.LogWarning("Abnormal MoM count for {dept}. Skipping.", dept);
                continue;
            }

            // PIC
            var momIds = moms.Select(x => x.MoMId).ToList();
            var allPics = await _momService.GetPICsForMomsAsync(momIds);
            var picGrouped = allPics.GroupBy(x => x.MoMId);
            var picRecipients = new List<string>();

            foreach (var group in picGrouped)
            {
                var mom = moms.FirstOrDefault(x => x.MoMId == group.Key);
                if (mom == null) continue;
                mom.PICs = group.Select(x => x.nama).Distinct().ToList();
                picRecipients.AddRange(group.Select(x => x.email));
            }

            // RECIPIENTS
            var deptHeadEmails = await _momService.GetDeptHeadEmailsAsync(dept);
            var sectHeadEmails = await _momService.GetSectHeadEmailsAsync(dept);
            var toRecipients = deptHeadEmails.Any() ? deptHeadEmails : sectHeadEmails;

            if (!toRecipients.Any())
            {
                _logger.LogWarning("No Dept Head or Sect Head found for {dept}", dept);
                continue;
            }

            var additionalPICDeptEmails = await _momService.GetAdditionalPICDeptEmailsAsync(momIds);

            var ccRecipients = sectHeadEmails
                .Concat(picRecipients)
                .Concat(additionalPICDeptEmails)
                .Concat(alwaysCcEmails)
                .Distinct()
                .Except(toRecipients)
                .ToList();

            var totalOutstanding = moms.Count;
            var overdueCount = moms.Count(x => x.DueDate1.HasValue && x.DueDate1.Value.Date < DateTime.Today);
            var openCount = moms.Count(x => x.Status == "OPEN");
            var onProgressCount = moms.Count(x => x.Status == "ON PROGRESS");

            string severityIcon = overdueCount > 0 ? "🔴" : openCount > 0 ? "🟡" : "🟢";

            var subject = $"{severityIcon} Pengingat MoM Level 2 ({typeLabel}) - Dept {dept} ({totalOutstanding} Outstanding, {overdueCount} Terlambat)";

            var body = Level2EmailTemplate.Generate(dept, moms, totalOutstanding, overdueCount, openCount, onProgressCount);

            _logger.LogInformation("Sending Level 2 {type} email (TO: {toCount}, CC: {ccCount}) for {dept}",
                typeLabel, toRecipients.Count, ccRecipients.Count, dept);

            await _emailService.SendAsync(toRecipients, ccRecipients, subject, body);

            emailSent += toRecipients.Count + ccRecipients.Count;

            await _logRepo.InsertAsync(notificationType, dept, DateTime.Today, moms.Count);

            _logger.LogInformation("Level 2 {type} email sent for {dept}", typeLabel, dept);
        }

        return emailSent;
    }
}