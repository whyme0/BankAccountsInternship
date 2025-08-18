using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace Api.HealthChecks;

public class RmqHealthCheck(IConfiguration config) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = config["RabbitMQ:Host"] ?? "rabbitmq",
                UserName = config["RabbitMQ:User"] ?? "admin",
                Password = config["RabbitMQ:Password"] ?? "admin"
            };

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            return await Task.FromResult(HealthCheckResult.Healthy("RabbitMQ доступен"));
        }
        catch (Exception ex)
        {
            return await Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ недоступен", ex));
        }
    }
}