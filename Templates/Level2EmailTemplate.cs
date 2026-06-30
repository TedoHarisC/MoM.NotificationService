using System.Text;
using MoM.NotificationService.Services;

namespace MoM.NotificationService.Templates;

public static class Level2EmailTemplate
{
    public static string Generate(string dept, List<MoMLevel2Dto> moms, int totalOutstanding,
    int overdueCount,
    int openCount,
    int onProgressCount)
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
                <td style='padding:8px;border:1px solid #ddd;'>{mom.CorrectiveAction}</td>
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
                                        Reminder MoM Level 2
                                    </h2>

                                    <p style='margin:0 0 10px 0;'>
                                        Kepada Yth. Dept Head, Sect Head & Tim <b>{dept}</b>,
                                    </p>

                                    <p style='margin:0 0 15px 0;'>
                                        Berikut adalah item MoM Level 2 yang belum selesai dan memerlukan perhatian sebelum meeting departemen berikutnya.
                                    </p>

                                    <table width='100%' cellpadding='0' cellspacing='0' style='margin-bottom:20px;'>
                                        <tr>
                                            <td align='center' style='padding:10px;background:#f8f9fa;border-radius:6px;'>
                                                <table width='100%' cellpadding='0' cellspacing='0'>
                                                    <tr>
                                                        <td align='center' style='padding:10px;'>
                                                            <div style='font-size:20px;font-weight:bold;color:#2c3e50;'>
                                                                {totalOutstanding}
                                                            </div>
                                                            <div style='font-size:12px;color:#666;'>Total Belum Selesai</div>
                                                        </td>
                                                        <td align='center' style='padding:10px;'>
                                                            <div style='font-size:20px;font-weight:bold;color:#d9534f;'>
                                                                {overdueCount}
                                                            </div>
                                                            <div style='font-size:12px;color:#666;'>Terlambat</div>
                                                        </td>
                                                        <td align='center' style='padding:10px;'>
                                                            <div style='font-size:20px;font-weight:bold;color:#2c3e50;'>
                                                                {openCount}
                                                            </div>
                                                            <div style='font-size:12px;color:#666;'>OPEN</div>
                                                        </td>
                                                        <td align='center' style='padding:10px;'>
                                                            <div style='font-size:20px;font-weight:bold;color:#2c3e50;'>
                                                                {onProgressCount}
                                                            </div>
                                                            <div style='font-size:12px;color:#666;'>ON PROGRESS</div>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>

                                    <table width='100%' cellpadding='0' cellspacing='0' 
                                           style='border-collapse:collapse;margin-top:15px;font-size:14px;'>

                                        <tr style='background-color:#2c3e50;color:#ffffff;'>
                                            <th style='padding:10px;border:1px solid #ddd;'>Topik</th>
                                            <th style='padding:10px;border:1px solid #ddd;'>Corrective Action</th>
                                            <th style='padding:10px;border:1px solid #ddd;'>Tanggal Jatuh Tempo</th>
                                            <th style='padding:10px;border:1px solid #ddd;'>Status</th>
                                            <th style='padding:10px;border:1px solid #ddd;'>Penanggung Jawab</th>
                                        </tr>

                                        {rows}

                                    </table>

                                    <p style='margin-top:20px;'>
                                        <strong>Catatan:</strong> Item yang ditandai dengan warna merah muda sudah melewati batas waktu.
                                    </p>

                                    <p style='margin-top:10px;'>
                                        Mohon dipastikan pembaruan progres diselesaikan sebelum meeting.
                                    </p>

                                    <hr style='margin-top:30px;border:none;border-top:1px solid #eee;' />

                                    <p style='font-size:12px;color:#888;margin-top:10px;'>
                                        Ini adalah notifikasi otomatis dari Sistem MoM (My Secretary).
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