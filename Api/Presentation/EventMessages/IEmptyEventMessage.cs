// ReSharper disable UnusedMemberInSuper.Global
namespace Api.Presentation.EventMessages;

public interface IEmptyEventMessage
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public Meta Meta { get; set; }
}