using Dapper;
using MoM.NotificationService.Dto;
using MoM.NotificationService.Repositories;
using MoM.NotificationService.Services;
using MoM.NotificationService.Templates;
using Quartz;

namespace MoM.NotificationService.Jobs;

[DisallowConcurrentExecution]
public class Level2PersonalizedReminderJob : IJob
{
    private readonly PersonalizedNotificationLogRepository _personalizedLogRepo;
    private readonly NotificationExecutionLogRepository _executionLogRepo;
    private readonly EmailService _emailService;
    private readonly MoMQueryService _momService;
    private readonly ILogger<Level2PersonalizedReminderJob> _logger;
    private readonly IConfiguration _configuration;

    public Level2PersonalizedReminderJob(
        ILogger<Level2PersonalizedReminderJob> logger,
        PersonalizedNotificationLogRepository personalizedLogRepo,
        NotificationExecutionLogRepository executionLogRepo,
        EmailService emailService,
        MoMQueryService momService,
        IConfiguration configuration)
    {
        _logger = logger;
        _personalizedLogRepo = personalizedLogRepo;
        _executionLogRepo = executionLogRepo;
        _emailService = emailService;
        _momService = momService;
        _configuration = configuration;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("🔥 Level 2 Personalized Reminder Job Triggered at {time}", DateTime.Now);

        var startTime = DateTime.Now;
        int executionId = await _executionLogRepo.InsertStartAsync("LEVEL2_PERSONALIZED", startTime);

        int totalEmailSent = 0;
        bool isSuccess = false;
        string? errorMessage = null;

        try
        {
            var alwaysCcNiks = _configuration.GetSection("NotificationSettings:AlwaysCcNiks").Get<List<string>>() ?? new();
            var alwaysCcEmails = await _momService.GetEmailsByNiksAsync(alwaysCcNiks);

            // === DAILY (ForumId=2) ===
            totalEmailSent += await SendPersonalizedAsync(isDaily: true, alwaysCcEmails, "L2P_DAILY", "Daily");

            // === WEEKLY (ForumId=3) ===
            totalEmailSent += await SendPersonalizedAsync(isDaily: false, alwaysCcEmails, "L2P_WEEKLY", "Weekly");

            isSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Level2PersonalizedReminderJob");
            isSuccess = false;
            errorMessage = ex.Message;
        }
        finally
        {
            await _executionLogRepo.UpdateEndAsync(executionId, DateTime.Now, isSuccess, totalEmailSent, errorMessage);
        }
    }

    private async Task<int> SendPersonalizedAsync(
        bool isDaily,
        List<string> alwaysCcEmails,
        string notificationType,
        string typeLabel)
    {
        int emailSent = 0;

        // === KIRIM KE PIC (karyawan biasa) ===
        var picData = await _momService.GetPersonalizedPICDataAsync(isDaily);

        // Group by PIC email
        var groupedByPIC = picData.GroupBy(x => x.EmailPIC);

        foreach (var picGroup in groupedByPIC)
        {
            var recipientEmail = picGroup.Key;

            // Filter MoM yang belum dikirim hari ini ke PIC ini
            var momsToSend = new List<PersonalizedPICRawDto>();
            foreach (var row in picGroup)
            {
                var alreadySent = await _personalizedLogRepo.ExistsAsync(notificationType, recipientEmail, row.MoMId);
                if (!alreadySent)
                    momsToSend.Add(row);
            }

            if (!momsToSend.Any()) continue;

            // Build email content
            var moms = momsToSend.Select(x => new MoMLevel2Dto
            {
                MoMId = x.MoMId,
                Topic = x.Topic,
                CorrectiveAction = x.CorrectiveAction,
                DueDate1 = x.DueDate1,
                Status = x.Status,
                Dept = x.Dept,
                LatestProgress = x.LatestProgress
            }).ToList();

            var totalOutstanding = moms.Count;
            var overdueCount = moms.Count(x => x.DueDate1.HasValue && x.DueDate1.Value.Date < DateTime.Today);
            var openCount = moms.Count(x => x.Status == "OPEN");
            var onProgressCount = moms.Count(x => x.Status == "ON PROGRESS");
            string severityIcon = overdueCount > 0 ? "🔴" : openCount > 0 ? "🟡" : "🟢";

            var subject = $"{severityIcon} Reminder MoM Level 2 ({typeLabel}) - {momsToSend.First().NamaPIC} ({totalOutstanding} Item)";
            var body = Level2EmailTemplate.Generate(moms[0].Dept, moms, totalOutstanding, overdueCount, openCount, onProgressCount);

            var toRecipients = new List<string> { recipientEmail };
            var ccRecipients = alwaysCcEmails.Except(toRecipients).Distinct().ToList();

            _logger.LogInformation("Sending {type} personalized PIC email to {email} ({count} MoM)", typeLabel, recipientEmail, moms.Count);

            await _emailService.SendAsync(toRecipients, ccRecipients, subject, body);
            emailSent++;

            // Catat log per MoM agar anti-spam per MoM
            foreach (var mom in momsToSend)
                await _personalizedLogRepo.InsertAsync(notificationType, recipientEmail, mom.MoMId);
        }

        // === KIRIM KE DH (Dept Head utama + Additional) ===
        var dhData = await _momService.GetPersonalizedDHDataAsync(isDaily);

        // Group by DH email
        var groupedByDH = dhData.GroupBy(x => x.EmailDH);

        foreach (var dhGroup in groupedByDH)
        {
            var recipientEmail = dhGroup.Key;
            var dhInfo = dhGroup.First();

            // Filter MoM yang belum dikirim hari ini ke DH ini
            var momsToSend = new List<PersonalizedDHRawDto>();
            foreach (var row in dhGroup)
            {
                var alreadySent = await _personalizedLogRepo.ExistsAsync(notificationType, recipientEmail, row.MoMId);
                if (!alreadySent)
                    momsToSend.Add(row);
            }

            if (!momsToSend.Any()) continue;

            // Build email content — MoM additional diberi tanda
            var moms = momsToSend.Select(x => new MoMLevel2Dto
            {
                MoMId = x.MoMId,
                Topic = x.IsAdditional
                    ? $"{x.Topic} ⚑ [Additional — PIC Utama: {x.PicDeptNama}]"
                    : x.Topic,
                CorrectiveAction = x.CorrectiveAction,
                DueDate1 = x.DueDate1,
                Status = x.Status,
                Dept = x.Dept,
                LatestProgress = x.LatestProgress
            }).ToList();

            var totalOutstanding = moms.Count;
            var overdueCount = moms.Count(x => x.DueDate1.HasValue && x.DueDate1.Value.Date < DateTime.Today);
            var openCount = moms.Count(x => x.Status == "OPEN");
            var onProgressCount = moms.Count(x => x.Status == "ON PROGRESS");
            string severityIcon = overdueCount > 0 ? "🔴" : openCount > 0 ? "🟡" : "🟢";

            var subject = $"{severityIcon} Reminder MoM Level 2 ({typeLabel}) - {dhInfo.NamaDH} ({totalOutstanding} Item)";
            var body = Level2EmailTemplate.Generate(dhInfo.Dept, moms, totalOutstanding, overdueCount, openCount, onProgressCount);

            var toRecipients = new List<string> { recipientEmail };
            var sectHeadEmails = await _momService.GetSectHeadEmailsByDeptAsync(dhInfo.Dept);
            var ccRecipients = sectHeadEmails
                .Concat(alwaysCcEmails)
                .Except(toRecipients)
                .Distinct()
                .ToList();

            _logger.LogInformation("Sending {type} personalized DH email to {email} ({count} MoM)", typeLabel, recipientEmail, moms.Count);

            await _emailService.SendAsync(toRecipients, ccRecipients, subject, body);
            emailSent++;

            // Catat log per MoM
            foreach (var mom in momsToSend)
                await _personalizedLogRepo.InsertAsync(notificationType, recipientEmail, mom.MoMId);
        }

        return emailSent;
    }
}
