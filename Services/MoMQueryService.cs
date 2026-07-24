using Dapper;
using Microsoft.Data.SqlClient;
using MoM.NotificationService.Dto;

namespace MoM.NotificationService.Services;

public class MoMQueryService
{
    private readonly string _connectionString;

    public MoMQueryService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found.");
    }

    public async Task<List<MoMDto>> GetOutstandingLevel1Async(string dept)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
                    SELECT
                        m.MoMId,
                        m.Topic,
                        m.CorrectiveAction,
                        m.DueDate1,
                        m.Status,
                        vw.anonim_dept AS PicDept,
                        ISNULL((
                            SELECT TOP 1 ProgressNote
                            FROM MoMProgress
                            WHERE MoMId = m.MoMId
                            ORDER BY CreatedDate DESC
                        ), '-') AS LatestProgress
                    FROM MoMs m
                    INNER JOIN db_site_sisfo.dbo.vw_detail_karyawan_aktif vw
                        ON vw.nik = m.PicDept
                    WHERE m.MoMLevel = 1
                    AND m.Status IN ('OPEN','ON PROGRESS')
                    AND m.IsDeleted = 0
                    AND vw.anonim_dept = @Dept
                    ORDER BY m.DueDate1 ASC";

        var result = await conn.QueryAsync<MoMDto>(sql, new { Dept = dept });
        var moms = result.ToList();

        if (moms.Any())
        {
            var momIds = moms.Select(x => x.MoMId).ToList();
            var attachments = await GetAttachmentsForMomsAsync(momIds);

            foreach (var mom in moms)
            {
                mom.Attachments = attachments
                    .Where(a => a.MoMId == mom.MoMId && !string.IsNullOrEmpty(a.FilePath))
                    .Select(a => new MoMAttachmentDto
                    {
                        AttachmentId = a.AttachmentId,
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        FileType = a.FileType
                    }).ToList();
            }
        }

        return moms;
    }

    public async Task<List<MoMLevel2Dto>> GetOutstandingLevel2Async()
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
                    SELECT
                        m.MoMId,
                        m.Topic,
                        m.CorrectiveAction,
                        m.DueDate1,
                        m.Status,
                        vw.anonim_dept AS Dept,
                        ISNULL((
                            SELECT TOP 1 ProgressNote
                            FROM MoMProgress
                            WHERE MoMId = m.MoMId
                            ORDER BY CreatedDate DESC
                        ), '-') AS LatestProgress
                    FROM MoMs m
                    INNER JOIN db_site_sisfo.dbo.vw_detail_karyawan_aktif vw
                        ON vw.nik = m.PicDept
                    WHERE m.MoMLevel = 2
                    AND m.Status IN ('OPEN','ON PROGRESS')
                    AND m.IsDeleted = 0
                    AND m.IssuedDate <= DATEADD(DAY, -5, GETDATE())";

        var result = await conn.QueryAsync<MoMLevel2Dto>(sql);
        var moms = result.ToList();

        if (moms.Any())
        {
            var momIds = moms.Select(x => x.MoMId).ToList();
            var attachments = await GetAttachmentsForMomsAsync(momIds);

            foreach (var mom in moms)
            {
                mom.Attachments = attachments
                    .Where(a => a.MoMId == mom.MoMId && !string.IsNullOrEmpty(a.FilePath))
                    .Select(a => new MoMAttachmentDto
                    {
                        AttachmentId = a.AttachmentId,
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        FileType = a.FileType
                    })
                    .ToList();
            }
        }

        return moms;
    }

    public async Task<List<MoMLevel2Dto>> GetOutstandingLevel2DailyAsync()
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
                    SELECT
                        m.MoMId,
                        m.Topic,
                        m.CorrectiveAction,
                        m.DueDate1,
                        m.Status,
                        m.ForumId,
                        vw.anonim_dept AS Dept,
                        ISNULL((
                            SELECT TOP 1 ProgressNote
                            FROM MoMProgress
                            WHERE MoMId = m.MoMId
                            ORDER BY CreatedDate DESC
                        ), '-') AS LatestProgress
                    FROM MoMs m
                    INNER JOIN db_site_sisfo.dbo.vw_detail_karyawan_aktif vw
                        ON vw.nik = m.PicDept
                    WHERE m.MoMLevel = 2
                    AND m.ForumId = 2
                    AND m.Status IN ('OPEN','ON PROGRESS')
                    AND m.IsDeleted = 0
                    AND DATEDIFF(DAY, m.IssuedDate, GETDATE()) % 3 = 0";

        var result = await conn.QueryAsync<MoMLevel2Dto>(sql);
        var moms = result.ToList();

        if (moms.Any())
        {
            var momIds = moms.Select(x => x.MoMId).ToList();
            var attachments = await GetAttachmentsForMomsAsync(momIds);

            foreach (var mom in moms)
            {
                mom.Attachments = attachments
                    .Where(a => a.MoMId == mom.MoMId && !string.IsNullOrEmpty(a.FilePath))
                    .Select(a => new MoMAttachmentDto
                    {
                        AttachmentId = a.AttachmentId,
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        FileType = a.FileType
                    })
                    .ToList();
            }
        }

        return moms;
    }

    public async Task<List<MoMLevel2Dto>> GetOutstandingLevel2WeeklyAsync()
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
                    SELECT
                        m.MoMId,
                        m.Topic,
                        m.CorrectiveAction,
                        m.DueDate1,
                        m.Status,
                        m.ForumId,
                        vw.anonim_dept AS Dept,
                        ISNULL((
                            SELECT TOP 1 ProgressNote
                            FROM MoMProgress
                            WHERE MoMId = m.MoMId
                            ORDER BY CreatedDate DESC
                        ), '-') AS LatestProgress
                    FROM MoMs m
                    INNER JOIN db_site_sisfo.dbo.vw_detail_karyawan_aktif vw
                        ON vw.nik = m.PicDept
                    WHERE m.MoMLevel = 2
                    AND m.ForumId = 3
                    AND m.Status IN ('OPEN','ON PROGRESS')
                    AND m.IsDeleted = 0
                    AND m.IssuedDate <= DATEADD(DAY, -5, GETDATE())";

        var result = await conn.QueryAsync<MoMLevel2Dto>(sql);
        var moms = result.ToList();

        if (moms.Any())
        {
            var momIds = moms.Select(x => x.MoMId).ToList();
            var attachments = await GetAttachmentsForMomsAsync(momIds);

            foreach (var mom in moms)
            {
                mom.Attachments = attachments
                    .Where(a => a.MoMId == mom.MoMId && !string.IsNullOrEmpty(a.FilePath))
                    .Select(a => new MoMAttachmentDto
                    {
                        AttachmentId = a.AttachmentId,
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        FileType = a.FileType
                    }).ToList();
            }
        }

        return moms;
    }

    public async Task<List<string>> GetLevel1RecipientsAsync(string dept)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
        SELECT DISTINCT email
        FROM db_site_sisfo.dbo.vw_detail_karyawan_aktif
        WHERE anonim_dept = @Dept
        AND jabatan LIKE '%Head%'
        AND status_karyawan = 'A'
        AND email IS NOT NULL";

        var result = await conn.QueryAsync<string>(sql, new { Dept = dept });

        return result.ToList();
    }

    public async Task<List<string>> GetPICEmailsAsync(int momId)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
                    SELECT DISTINCT vw.email
                    FROM MoMPICEmployees pic
                    INNER JOIN db_site_sisfo.dbo.vw_detail_karyawan_aktif vw
                        ON vw.id_karyawan = pic.UserId
                    WHERE pic.MoMId = @MoMId
                    AND pic.IsDeleted = 0
                    AND vw.status_karyawan = 'A'
                    AND vw.email IS NOT NULL";

        var result = await conn.QueryAsync<string>(sql, new { MoMId = momId });
        return result.ToList();
    }

    public async Task<List<string>> GetPICNamesAsync(int momId)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
        SELECT DISTINCT vw.nama
        FROM MoMPICEmployees pic
        INNER JOIN db_site_sisfo.dbo.vw_detail_karyawan_aktif vw
            ON vw.nik = pic.UserId
        WHERE pic.MoMId = @MoMId
        AND pic.IsDeleted = 0
        AND vw.status_karyawan = 'A'";

        var result = await conn.QueryAsync<string>(sql, new { MoMId = momId });

        return result.ToList();
    }

    public async Task<List<Level2PICRawDto>> GetPICsForMomsAsync(List<int> momIds)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
        SELECT
            pic.MoMId,
            vw.email,
            vw.nama
        FROM MoMPICEmployees pic
        INNER JOIN db_site_sisfo.dbo.vw_detail_karyawan_aktif vw
            ON vw.nik = pic.UserId
        WHERE pic.MoMId IN @MoMIds
        AND pic.IsDeleted = 0
        AND vw.status_karyawan = 'A'
        AND vw.email IS NOT NULL";

        var result = await conn.QueryAsync<Level2PICRawDto>(sql, new { MoMIds = momIds });

        return result.ToList();
    }

    public async Task<List<AttachmentRawDto>> GetAttachmentsForMomsAsync(List<int> momIds)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
        SELECT
            AttachmentId,
            MoMId,
            FileName,
            FilePath,
            FileType
        FROM MoMAttachments
        WHERE MoMId IN @MoMIds
        AND IsDeleted = 0
        ORDER BY UploadedDate ASC";

        var result = await conn.QueryAsync<AttachmentRawDto>(sql, new { MoMIds = momIds });

        return result.ToList();
    }

    public async Task<List<string>> GetDeptHeadEmailsAsync(string dept)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
        SELECT DISTINCT email
        FROM db_site_sisfo.dbo.vw_detail_karyawan_aktif
        WHERE anonim_dept = @Dept
        AND anonim_jabatan = 'DH'
        AND status_karyawan = 'A'
        AND email IS NOT NULL";

        var result = await conn.QueryAsync<string>(sql, new { Dept = dept });
        return result.ToList();
    }

    public async Task<List<string>> GetSectHeadEmailsAsync(string dept)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
        SELECT DISTINCT email
        FROM db_site_sisfo.dbo.vw_detail_karyawan_aktif
        WHERE anonim_dept = @Dept
        AND anonim_jabatan = 'SH'
        AND status_karyawan = 'A'
        AND email IS NOT NULL";

        var result = await conn.QueryAsync<string>(sql, new { Dept = dept });
        return result.ToList();
    }

    public async Task<List<string>> GetGMEmailsAsync()
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
        SELECT DISTINCT email
        FROM db_site_sisfo.dbo.vw_detail_karyawan_aktif
        WHERE anonim_jabatan = 'GM'
        AND status_karyawan = 'A'
        AND email IS NOT NULL";

        var result = await conn.QueryAsync<string>(sql);

        return result.ToList();
    }

    public async Task<List<string>> GetEmailsByNiksAsync(IEnumerable<string> niks)
    {
        var nikList = niks.ToList();
        if (!nikList.Any()) return new List<string>();

        using var conn = new SqlConnection(_connectionString);

        var sql = @"
        SELECT DISTINCT email
        FROM db_site_sisfo.dbo.vw_detail_karyawan_aktif
        WHERE nik IN @Niks
        AND status_karyawan = 'A'
        AND email IS NOT NULL";

        var result = await conn.QueryAsync<string>(sql, new { Niks = nikList });
        return result.ToList();
    }

    public async Task<List<string>> GetAdditionalPICDeptEmailsAsync(List<int> momIds)
    {
        if (!momIds.Any()) return new List<string>();

        using var conn = new SqlConnection(_connectionString);

        var sql = @"
        SELECT DISTINCT vw.email
        FROM MoMAdditionalPICDept apd
        INNER JOIN db_site_sisfo.dbo.vw_detail_karyawan_aktif vw
            ON vw.nik = apd.DeptHeadNik
        WHERE apd.MoMId IN @MoMIds
        AND apd.IsDeleted = 0
        AND vw.status_karyawan = 'A'
        AND vw.email IS NOT NULL";

        var result = await conn.QueryAsync<string>(sql, new { MoMIds = momIds });
        return result.ToList();
    }
}

public class MoMDto
{
    public int MoMId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string CorrectiveAction { get; set; } = string.Empty;
    public DateTime? DueDate1 { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PicDept { get; set; } = string.Empty;
    public string LatestProgress { get; set; } = "-";
    public List<MoMAttachmentDto> Attachments { get; set; } = new();
}