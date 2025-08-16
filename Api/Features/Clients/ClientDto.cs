// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PreferConcreteValueOverDefault
namespace Api.Features.Clients;

/// <summary>
/// Представляет данные клиента
/// </summary>
public class ClientDto
{
    /// <summary>
    /// Уникальный идентификатор клиента
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Имя клиента
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Заморожен ли счет клиента
    /// </summary>
    public bool Frozen { get; set; }
}