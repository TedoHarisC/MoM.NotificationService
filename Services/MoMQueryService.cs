using Dapper;
using Microsoft.Data.SqlClient;

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
                        vw.anonim_dept AS PicDept
                    FROM MoMs m
                    INNER JOIN db_site_sisfo.dbo.vw_detail_karyawan_aktif vw
                        ON vw.nik = m.PicDept
                    WHERE m.MoMLevel = 1
                    AND m.Status IN ('OPEN','ON PROGRESS')
                    AND m.IsDeleted = 0
                    AND vw.anonim_dept = @Dept
                    ORDER BY m.DueDate1 ASC";

        var result = await conn.QueryAsync<MoMDto>(sql, new { Dept = dept });

        return result.ToList();
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
                        vw.anonim_dept AS Dept
                    FROM MoMs m
                    INNER JOIN db_site_sisfo.dbo.vw_detail_karyawan_aktif vw
                        ON vw.nik = m.PicDept
                    WHERE m.MoMLevel = 2
                    AND m.Status IN ('OPEN','ON PROGRESS')
                    AND m.IsDeleted = 0
                    AND m.IssuedDate <= DATEADD(DAY, -5, GETDATE())";

        var result = await conn.QueryAsync<MoMLevel2Dto>(sql);
        return result.ToList();
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
}

public class MoMDto
{
    public int MoMId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string CorrectiveAction { get; set; } = string.Empty;
    public DateTime? DueDate1 { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PicDept { get; set; } = string.Empty;
}