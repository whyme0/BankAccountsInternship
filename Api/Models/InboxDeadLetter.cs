// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable RedundantDefaultMemberInitializer
namespace Api.Models;

public class InboxDeadLetter
{
    public Guid Id { get; set; }
    public string? MessageId { get; set; } = null!;
    public DateTime ReceivedAt { get; set; }
    public string Handler { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public string Error { get; set; } = null!;
}