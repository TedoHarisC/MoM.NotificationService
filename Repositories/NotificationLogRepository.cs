using Dapper;
using Microsoft.Data.SqlClient;

namespace MoM.NotificationService.Repositories;

public class NotificationLogRepository
{
    private readonly IConfiguration _config;
    private readonly string _connectionString;

    public NotificationLogRepository(IConfiguration config)
    {
        _config = config;
        _connectionString = config.GetConnectionString("DefaultConnection")
       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<bool> ExistsAsync(string level, string dept, DateTime periodDate)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
            SELECT 1
            FROM MoMNotificationLogs
            WHERE NotificationType = @Level
            AND DeptCode = @Dept
            AND PeriodDate = @PeriodDate";

        var result = await conn.QueryFirstOrDefaultAsync<int?>(
            sql,
            new { Level = level, Dept = dept, PeriodDate = periodDate.Date });

        return result.HasValue;
    }

    public async Task InsertAsync(string level, string dept, DateTime periodDate, int totalMom)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
            INSERT INTO MoMNotificationLogs
            (NotificationType, DeptCode, PeriodDate, SentDate, TotalMoM)
            VALUES
            (@Level, @Dept, @PeriodDate, GETDATE(), @TotalMoM)";

        await conn.ExecuteAsync(sql, new
        {
            Level = level,
            Dept = dept,
            PeriodDate = periodDate.Date,
            TotalMoM = totalMom
        });
    }

    public async Task<int> DeleteOlderThanAsync(int months)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
        DELETE FROM MoMNotificationLogs
        WHERE PeriodDate < DATEADD(MONTH, -@Months, GETDATE())";

        var affected = await conn.ExecuteAsync(sql, new { Months = months });

        return affected;
    }
}