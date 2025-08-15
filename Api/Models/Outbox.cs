// ReSharper disable PreferConcreteValueOverDefault
namespace Api.Models;

public class Outbox
{
    public Guid Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Type { get; set; } = default!;
    public string RoutingKey { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime? PublishedAt { get; set; }
}