using Npgsql;

namespace HangfireJobs.Jobs;

public class AccrueInterest(IConfiguration configuration) : IJob
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(
            configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync(cancellationToken);
        
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var accountIds = new List<Guid>();
        await using (var cmd = new NpgsqlCommand(
                         """
                         SELECT "Id" FROM public."Accounts" 
                                       WHERE "Type" = 1
                         """, connection, transaction))
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
                accountIds.Add(reader.GetGuid(0));
        }

        foreach (var accountId in accountIds)
        {
            await using var cmd = new NpgsqlCommand(
                "CALL accrue_interest(@account_id)", connection, transaction);
            cmd.Parameters.AddWithValue("account_id", accountId);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}