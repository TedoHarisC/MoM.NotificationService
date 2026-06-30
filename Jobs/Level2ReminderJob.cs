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
            var allMoms = await _momService.GetOutstandingLevel2Async();

            if (!allMoms.Any())
            {
                _logger.LogInformation("No outstanding Level 2 MoM found.");
                isSuccess = true;
            }
            else
            {
                var groupedByDept = allMoms.GroupBy(x => x.Dept);

                foreach (var deptGroup in groupedByDept)
                {
                    var dept = deptGroup.Key;
                    var moms = deptGroup.ToList();

                    var alreadySent = await _logRepo.ExistsAsync("LEVEL2", dept, DateTime.Today);
                    if (alreadySent)
                    {
                        _logger.LogInformation("Already sent Level 2 reminder for {dept}", dept);
                        continue;
                    }

                    if (moms.Count > 200)
                    {
                        _logger.LogWarning("Abnormal MoM count detected for {dept}. Skipping.", dept);
                        continue;
                    }

                    // PIC OPTIMIZED QUERY
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

                    // RECIPIENT LOGIC
                    var deptHeadEmails = await _momService.GetDeptHeadEmailsAsync(dept);
                    var sectHeadEmails = await _momService.GetSectHeadEmailsAsync(dept);

                    // Fallback ke SH jika tidak ada DH
                    var toRecipients = deptHeadEmails.Any() ? deptHeadEmails : sectHeadEmails;

                    if (!toRecipients.Any())
                    {
                        _logger.LogWarning("No Dept Head or Sect Head found for {dept}", dept);
                        continue;
                    }

                    var alwaysCcNiks = _configuration.GetSection("NotificationSettings:AlwaysCcNiks").Get<List<string>>() ?? new();
                    var alwaysCcEmails = await _momService.GetEmailsByNiksAsync(alwaysCcNiks);

                    var ccRecipients = sectHeadEmails
                        .Concat(picRecipients)
                        .Concat(alwaysCcEmails)
                        .Distinct()
                        .Except(toRecipients)
                        .ToList();

                    // SUMMARY CALCULATION
                    var totalOutstanding = moms.Count;
                    var overdueCount = moms.Count(x =>
                        x.DueDate1.HasValue &&
                        x.DueDate1.Value.Date < DateTime.Today);

                    var openCount = moms.Count(x => x.Status == "OPEN");
                    var onProgressCount = moms.Count(x => x.Status == "ON PROGRESS");

                    string severityIcon =
                        overdueCount > 0 ? "🔴" :
                        openCount > 0 ? "🟡" : "🟢";

                    var subject =
                        $"{severityIcon} Pengingat MoM Level 2 - Dept {dept} ({totalOutstanding} Outstanding, {overdueCount} Terlambat)";

                    var body = Level2EmailTemplate.Generate(
                        dept,
                        moms,
                        totalOutstanding,
                        overdueCount,
                        openCount,
                        onProgressCount);

                    _logger.LogInformation(
                        "Sending Level 2 email (TO: {toCount}, CC: {ccCount}) for {dept}",
                        toRecipients.Count,
                        ccRecipients.Count,
                        dept);

                    await _emailService.SendAsync(toRecipients, ccRecipients, subject, body);

                    totalEmailSent += toRecipients.Count + ccRecipients.Count;

                    await _logRepo.InsertAsync("LEVEL2", dept, DateTime.Today, moms.Count);

                    _logger.LogInformation("Level 2 email sent for {dept}", dept);
                }

                isSuccess = true;
            }
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
}