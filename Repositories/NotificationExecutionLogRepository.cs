using Dapper;
using Microsoft.Data.SqlClient;

namespace MoM.NotificationService.Repositories;

public class NotificationExecutionLogRepository
{
    private readonly string _connectionString;

    public NotificationExecutionLogRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found.");
    }

    public async Task<int> InsertStartAsync(string jobName, DateTime startTime)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
            INSERT INTO MoMNotificationExecutionLogs
            (JobName, StartTime, IsSuccess)
            VALUES (@JobName, @StartTime, 0);

            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            JobName = jobName,
            StartTime = startTime
        });
    }

    public async Task UpdateEndAsync(int id, DateTime endTime, bool isSuccess, int totalEmailSent, string? errorMessage)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
            UPDATE MoMNotificationExecutionLogs
            SET EndTime = @EndTime,
                IsSuccess = @IsSuccess,
                TotalEmailSent = @TotalEmailSent,
                ErrorMessage = @ErrorMessage
            WHERE Id = @Id";

        await conn.ExecuteAsync(sql, new
        {
            Id = id,
            EndTime = endTime,
            IsSuccess = isSuccess,
            TotalEmailSent = totalEmailSent,
            ErrorMessage = errorMessage
        });
    }
}