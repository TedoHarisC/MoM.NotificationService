using MoM.NotificationService.Services;
using System.Text;
namespace MoM.NotificationService.Templates;

public static class Level1EmailTemplate
{
    public static string Generate(string dept, List<MoMDto> moms)
    {
        var sb = new StringBuilder();

        foreach (var mom in moms)
        {
            var isOverdue = mom.DueDate1.HasValue && mom.DueDate1.Value.Date < DateTime.Today;
            var rowColor = isOverdue ? "#ffe6e6" : "#ffffff";
            var statusColor = mom.Status == "OPEN" ? "#d9534f" : "#f0ad4e";

            sb.Append($@"
            <tr style='background-color:{rowColor};'>
                <td style='padding:8px;border:1px solid #ddd;'>{mom.Topic}</td>
                <td style='padding:8px;border:1px solid #ddd;'>{mom.DueDate1:dd MMM yyyy}</td>
                <td style='padding:8px;border:1px solid #ddd;color:{statusColor};font-weight:bold;'>
                    {mom.Status}
                </td>
            </tr>");
        }

        return $@"
        <html>
        <body style='font-family:Arial, sans-serif; background-color:#f4f6f9; padding:20px;'>
            <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                    <td align='center'>
                        <table width='700' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:6px;padding:20px;'>
                            <tr>
                                <td>
                                    <h2 style='color:#2c3e50;'>MoM Level 1 Reminder</h2>
                                    <p>Dear Dept Head & Sect Head <b>{dept}</b>,</p>
                                    <p>Below are outstanding MoM items that require attention:</p>

                                    <table width='100%' cellpadding='0' cellspacing='0' style='border-collapse:collapse; margin-top:15px;'>
                                        <tr style='background-color:#2c3e50;color:#ffffff;'>
                                            <th style='padding:10px;border:1px solid #ddd;'>Topic</th>
                                            <th style='padding:10px;border:1px solid #ddd;'>Due Date</th>
                                            <th style='padding:10px;border:1px solid #ddd;'>Status</th>
                                        </tr>
                                        {sb}
                                    </table>

                                    <p style='margin-top:20px;'>
                                        Please ensure completion before the upcoming meeting.
                                    </p>

                                    <hr style='margin-top:30px;'/>
                                    <p style='font-size:12px;color:#888;'>
                                        This is an automated message from MoM System.
                                    </p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>";
    }
}