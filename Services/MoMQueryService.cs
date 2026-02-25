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
}

public class MoMDto
{
    public int MoMId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public DateTime? DueDate1 { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PicDept { get; set; } = string.Empty;
}