// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
namespace Api.Models;

public class AuditEvent
{
    public Guid Id { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string RoutingKey { get; set; } = null!;
    public string Payload { get; set; } = null!;
}