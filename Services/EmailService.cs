using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MoM.NotificationService.Services;

public class EmailService
{
    private readonly string _senderEmail;
    private readonly string _password;
    private readonly bool _isTestMode;
    private readonly string? _testEmail;

    public EmailService(IConfiguration config)
    {
        _senderEmail = config["EmailSettings:SenderEmail"]
            ?? throw new InvalidOperationException("SenderEmail not configured.");

        _password = config["EmailSettings:Password"]
            ?? throw new InvalidOperationException("Email password not configured.");

        _isTestMode = bool.Parse(config["NotificationSettings:IsTestMode"] ?? "true");
        _testEmail = config["NotificationSettings:TestEmail"];
    }

    public async Task SendAsync(List<string> toRecipients, List<string> ccRecipients, string subject, string htmlBody)
    {
        if (_isTestMode)
        {
            Console.WriteLine("🚨 TEST MODE ACTIVE");
            Console.WriteLine($"Original recipients count: {toRecipients.Count}");

            toRecipients = new List<string> { _testEmail! };
            ccRecipients = new List<string>();
            subject = "[TEST MODE] " + subject;

            Console.WriteLine($"Final recipients: {_testEmail}");
        }

        if (toRecipients == null || !toRecipients.Any())
            throw new ArgumentException("Recipient list cannot be empty.");

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("MoM System", _senderEmail));

        foreach (var email in toRecipients.Distinct())
        {
            message.To.Add(MailboxAddress.Parse(email));
        }

        foreach (var email in ccRecipients.Distinct())
        {
            message.Cc.Add(MailboxAddress.Parse(email));
        }

        message.Subject = subject;
        message.Body = new TextPart("html")
        {
            Text = htmlBody
        };

        using var client = new SmtpClient();

        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_senderEmail, _password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}