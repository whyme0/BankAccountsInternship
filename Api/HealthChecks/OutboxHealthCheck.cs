using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.HealthChecks;

public class OutboxHealthCheck(AppDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var count = await db.Outbox.CountAsync(m => m.PublishedAt == null, cancellationToken);

        return count > 100 ? HealthCheckResult.Degraded($"Outbox отстаёт: {count} неопубликованных сообщений") : HealthCheckResult.Healthy($"Outbox в норме: {count} неопубликованных сообщений");
    }

}