// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace Api.Presentation.EventMessages;

/// <summary>
/// Событие разблокировки клиента
/// </summary>
public class ClientUnblocked
{
    /// <summary>
    /// Идентификатор клиента
    /// </summary>
    public Guid ClientId { get; set; }
}