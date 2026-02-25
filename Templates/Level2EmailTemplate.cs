using System.Text;
using MoM.NotificationService.Services;

namespace MoM.NotificationService.Templates;

public static class Level2EmailTemplate
{
    public static string Generate(string dept, List<MoMLevel2Dto> moms)
    {
        var rows = new StringBuilder();

        foreach (var mom in moms)
        {
            var isOverdue = mom.DueDate1.HasValue && mom.DueDate1.Value.Date < DateTime.Today;
            var rowColor = isOverdue ? "#ffe6e6" : "#ffffff";
            var statusColor = mom.Status == "OPEN" ? "#d9534f" : "#f0ad4e";

            var picList = mom.PICs != null && mom.PICs.Any()
                ? string.Join("<br/>", mom.PICs)
                : "-";

            rows.Append($@"
            <tr style='background-color:{rowColor};'>
                <td style='padding:8px;border:1px solid #ddd;'>{mom.Topic}</td>
                <td style='padding:8px;border:1px solid #ddd;'>{mom.DueDate1:dd MMM yyyy}</td>
                <td style='padding:8px;border:1px solid #ddd;color:{statusColor};font-weight:bold;'>
                    {mom.Status}
                </td>
                <td style='padding:8px;border:1px solid #ddd;'>{picList}</td>
            </tr>");
        }

        return $@"
        <html>
        <body style='font-family:Arial, sans-serif; background-color:#f4f6f9; padding:20px;'>
            <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                    <td align='center'>
                        <table width='750' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:6px;padding:25px;'>
                            <tr>
                                <td>

                                    <h2 style='color:#2c3e50;margin-bottom:10px;'>
                                        MoM Level 2 Reminder
                                    </h2>

                                    <p style='margin:0 0 10px 0;'>
                                        Dear Dept Head, Sect Head & Team <b>{dept}</b>,
                                    </p>

                                    <p style='margin:0 0 15px 0;'>
                                        Below are outstanding MoM Level 2 items that require attention before the upcoming departmental meeting.
                                    </p>

                                    <table width='100%' cellpadding='0' cellspacing='0' 
                                           style='border-collapse:collapse;margin-top:15px;font-size:14px;'>

                                        <tr style='background-color:#2c3e50;color:#ffffff;'>
                                            <th style='padding:10px;border:1px solid #ddd;'>Topic</th>
                                            <th style='padding:10px;border:1px solid #ddd;'>Due Date</th>
                                            <th style='padding:10px;border:1px solid #ddd;'>Status</th>
                                            <th style='padding:10px;border:1px solid #ddd;'>PIC</th>
                                        </tr>

                                        {rows}

                                    </table>

                                    <p style='margin-top:20px;'>
                                        <strong>Note:</strong> Items highlighted in light red are overdue.
                                    </p>

                                    <p style='margin-top:10px;'>
                                        Kindly ensure progress updates are completed prior to the meeting.
                                    </p>

                                    <hr style='margin-top:30px;border:none;border-top:1px solid #eee;' />

                                    <p style='font-size:12px;color:#888;margin-top:10px;'>
                                        This is an automated notification from the MoM System.
                                        <br/>
                                        Please do not reply to this email.
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