using Dapper;
using Microsoft.Data.SqlClient;

namespace MoM.NotificationService.Repositories;

public class PersonalizedNotificationLogRepository
{
    private readonly string _connectionString;

    public PersonalizedNotificationLogRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found.");
    }

    public async Task<bool> ExistsAsync(string notificationType, string recipientEmail, int momId)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
            SELECT 1
            FROM MoMPersonalizedNotificationLogs
            WHERE NotificationType = @NotificationType
            AND RecipientEmail = @RecipientEmail
            AND MoMId = @MoMId
            AND SentDate = @SentDate";

        var result = await conn.QueryFirstOrDefaultAsync<int?>(sql, new
        {
            NotificationType = notificationType,
            RecipientEmail = recipientEmail,
            MoMId = momId,
            SentDate = DateTime.Today
        });

        return result.HasValue;
    }

    public async Task InsertAsync(string notificationType, string recipientEmail, int momId)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
            INSERT INTO MoMPersonalizedNotificationLogs
            (NotificationType, RecipientEmail, MoMId, SentDate, CreatedDate)
            VALUES
            (@NotificationType, @RecipientEmail, @MoMId, @SentDate, GETDATE())";

        await conn.ExecuteAsync(sql, new
        {
            NotificationType = notificationType,
            RecipientEmail = recipientEmail,
            MoMId = momId,
            SentDate = DateTime.Today
        });
    }

    public async Task<int> DeleteOlderThanAsync(int months)
    {
        using var conn = new SqlConnection(_connectionString);

        var sql = @"
            DELETE FROM MoMPersonalizedNotificationLogs
            WHERE SentDate < DATEADD(MONTH, -@Months, GETDATE())";

        return await conn.ExecuteAsync(sql, new { Months = months });
    }
}
