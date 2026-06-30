using MoM.NotificationService.Services;
using System.Text;

namespace MoM.NotificationService.Templates;

public static class Level1EmailTemplate
{
    public static string Generate(
        string dept,
        List<MoMDto> moms,
        int totalOutstanding,
        int overdueCount)
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
                <td style='padding:8px;border:1px solid #ddd;'>{mom.CorrectiveAction}</td>
                <td style='padding:8px;border:1px solid #ddd;'>{mom.DueDate1:dd MMM yyyy}</td>
                <td style='padding:8px;border:1px solid #ddd;color:{statusColor};font-weight:bold;'>
                    {mom.Status}
                </td>
            </tr>");
        }

        double overduePercentage = totalOutstanding == 0
        ? 0
        : (double)overdueCount / totalOutstanding;

        string summaryBackground;
        string severityTextColor;

        if (overduePercentage > 0.5)
        {
            summaryBackground = "#ffe6e6"; // merah lembut
            severityTextColor = "#d9534f";
        }
        else if (overdueCount > 0)
        {
            summaryBackground = "#fff4e5"; // kuning lembut
            severityTextColor = "#f0ad4e";
        }
        else
        {
            summaryBackground = "#e6f4ea"; // hijau lembut
            severityTextColor = "#28a745";
        }

        return $@"
        <html>
        <body style='font-family:Arial, sans-serif; background-color:#f4f6f9; padding:20px;'>
            <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                    <td align='center'>
                        <table width='700' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:6px;padding:25px;'>
                            <tr>
                                <td>

                                    <h2 style='color:#2c3e50;margin-bottom:10px;'>
                                        Pengingat MoM Level 1
                                    </h2>

                                    <p style='margin:0 0 10px 0;'>
                                        Yth. Dept Head & Sect Head <b>{dept}</b>,
                                    </p>

                                    <p style='margin:0 0 15px 0;'>
                                        Berikut adalah daftar MoM Level 1 yang masih outstanding dan memerlukan perhatian sebelum meeting berikutnya.
                                    </p>

                                    <!-- SUMMARY BOX -->
                                    <table width='100%' cellpadding='0' cellspacing='0' style='margin-bottom:20px;'>
                                        <tr>
                                            <td align='center' style='padding:10px;background:{summaryBackground};border-radius:6px;'>
                                                <table width='100%' cellpadding='0' cellspacing='0'>
                                                    <tr>
                                                        <td align='center' style='padding:10px;'>
                                                            <div style='font-size:22px;font-weight:bold;color:#2c3e50;'>
                                                                {totalOutstanding}
                                                            </div>
                                                            <div style='font-size:12px;color:#666;'>
                                                                Total Outstanding
                                                            </div>
                                                        </td>
                                                        <td align='center' style='padding:10px;'>
                                                            <div style='font-size:22px;font-weight:bold;color:{severityTextColor};'>
                                                                {overdueCount}
                                                            </div>
                                                            <div style='font-size:12px;color:#666;'>
                                                                Total Terlambat
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- TABLE DETAIL -->
                                    <table width='100%' cellpadding='0' cellspacing='0' 
                                           style='border-collapse:collapse;margin-top:15px;font-size:14px;'>

                                        <tr style='background-color:#2c3e50;color:#ffffff;'>
                                            <th style='padding:10px;border:1px solid #ddd;'>Topik</th>
                                            <th style='padding:10px;border:1px solid #ddd;'>Corrective Action</th>
                                            <th style='padding:10px;border:1px solid #ddd;'>Tanggal Jatuh Tempo</th>
                                            <th style='padding:10px;border:1px solid #ddd;'>Status</th>
                                        </tr>

                                        {sb}

                                    </table>

                                    <p style='margin-top:20px;'>
                                        <strong>Catatan:</strong> Item yang ditandai warna merah muda berarti telah melewati batas waktu.
                                    </p>

                                    <p style='margin-top:10px;'>
                                        Mohon dipastikan penyelesaiannya sebelum meeting berikutnya.
                                    </p>

                                    <hr style='margin-top:30px;border:none;border-top:1px solid #eee;' />

                                    <p style='font-size:12px;color:#888;margin-top:10px;'>
                                        Email ini dikirim secara otomatis oleh Sistem MoM (My Secretary).
                                        <br/>
                                        Mohon tidak membalas email ini.
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