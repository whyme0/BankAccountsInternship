// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace Api.Presentation.EventMessages;

/// <summary>
/// Событие блокировки клиента
/// </summary>
public class ClientBlocked
{
    /// <summary>
    /// Идентификатор клиента
    /// </summary>
    public Guid ClientId { get; set; }
}