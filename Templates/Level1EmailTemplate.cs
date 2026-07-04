using MoM.NotificationService.Services;
using MoM.NotificationService.Dto;
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
                <td style='padding:8px;border:1px solid #ddd;'>{mom.LatestProgress}</td>
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
                                            <th style='padding:10px;border:1px solid #ddd;'>Progress Terakhir</th>
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

                                    {GenerateAttachmentsSection(moms)}

                                    <hr style='margin-top:30px;border:none;border-top:1px solid #ddd;' />

                                    <div style='padding:15px;background:#e8f4f8;border-left:4px solid #2196F3;border-radius:4px;margin-top:20px;'>
                                        <h4 style='color:#2c3e50;margin:0 0 10px 0;font-size:14px;'>
                                            📋 Cara Follow Up MoM
                                        </h4>
                                        <ol style='margin:0;padding-left:20px;font-size:13px;color:#555;line-height:1.6;'>
                                            <li>Login ke <strong>Sisfo</strong> di <a href='http://10.2.182.50' target='_blank' style='color:#2196F3;text-decoration:none;'>10.2.182.50</a></li>
                                            <li>Pilih menu <strong>Vortex</strong></li>
                                            <li>Pilih menu <strong>MoM</strong> → <strong>Level 1</strong></li>
                                            <li>Isikan <strong>Progress</strong> terkini dan upload <strong>Attachment</strong> jika diperlukan</li>
                                        </ol>
                                        <p style='margin:10px 0 0 0;font-size:12px;color:#666;'>
                                            💡 <em>Progress yang diupdate akan muncul di email reminder berikutnya</em>
                                        </p>
                                    </div>

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

    private static string GenerateAttachmentsSection(List<MoMDto> moms)
    {
        var allAttachments = moms.SelectMany(m => m.Attachments
            .Select(a => new { MoM = m, Attachment = a }))
            .ToList();

        if (!allAttachments.Any())
            return string.Empty;

        var sb = new StringBuilder();
        sb.Append(@"
                                    <hr style='margin-top:25px;margin-bottom:20px;border:none;border-top:1px solid #ddd;' />
                                    <h3 style='color:#2c3e50;margin-bottom:15px;font-size:16px;'>
                                        📎 Lampiran Dokumen
                                    </h3>");

        foreach (var group in moms.Where(m => m.Attachments.Any()))
        {
            sb.Append($@"
                                    <div style='margin-bottom:20px;padding:15px;background:#f8f9fa;border-radius:6px;'>
                                        <div style='font-weight:bold;color:#2c3e50;margin-bottom:12px;font-size:14px;'>
                                            {group.Topic}
                                        </div>
                                        <div style='display:table;width:100%;'>");

            var attachmentIndex = 0;
            foreach (var att in group.Attachments)
            {
                if (attachmentIndex % 3 == 0 && attachmentIndex > 0)
                {
                    sb.Append(@"</div><div style='display:table;width:100%;margin-top:10px;'>");
                }

                if (att.IsImage)
                {
                    sb.Append($@"
                                            <div style='display:table-cell;width:33%;padding:5px;text-align:center;vertical-align:top;'>
                                                <div style='margin-bottom:6px;'>
                                                    <span style='color:#666;font-size:12px;'>🖼️ {att.FileName}</span>
                                                </div>
                                                <a href='{att.FullUrl}' target='_blank' style='text-decoration:none;'>
                                                    <img src='{att.FullUrl}'
                                                         alt='{att.FileName}'
                                                         style='width:200px;height:140px;border:1px solid #ddd;border-radius:4px;object-fit:cover;' />
                                                </a>
                                                <a href='{att.FullUrl}' target='_blank'
                                                   style='display:inline-block;margin-top:6px;padding:5px 10px;background:#007bff;color:#fff;text-decoration:none;border-radius:3px;font-size:11px;'>
                                                    📥 Download
                                                </a>
                                            </div>");
                }
                else
                {
                    var icon = att.FileType.ToLower() == "pdf" ? "📄" : "📎";
                    sb.Append($@"
                                            <div style='display:table-cell;width:33%;padding:5px;text-align:center;vertical-align:top;'>
                                                <div style='margin-bottom:6px;'>
                                                    <span style='color:#666;font-size:12px;'>{icon} {att.FileName}</span>
                                                </div>
                                                <div style='width:200px;height:140px;border:1px solid #ddd;border-radius:4px;display:inline-flex;align-items:center;justify-content:center;background:#fff;'>
                                                    <div style='text-align:center;'>
                                                        <div style='font-size:36px;margin-bottom:6px;'>{icon}</div>
                                                        <div style='font-size:10px;color:#666;'>Document</div>
                                                    </div>
                                                </div>
                                                <a href='{att.FullUrl}' target='_blank'
                                                   style='display:inline-block;margin-top:6px;padding:5px 10px;background:#28a745;color:#fff;text-decoration:none;border-radius:3px;font-size:11px;'>
                                                    📥 Download
                                                </a>
                                            </div>");
                }

                attachmentIndex++;
            }

            sb.Append(@"
                                        </div>
                                    </div>");
        }

        return sb.ToString();
    }
}