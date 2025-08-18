// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PreferConcreteValueOverDefault
namespace Api.Models;

/// <summary>
/// Данная сущность выступает скорее в роли заглушки, нежели чем действительная сущность бизнес логики.<br/>
/// В дальнейшем вероятней всего необходим будет переход на Client с ролями
/// </summary>
public class Client
{
    /// <summary>
    /// Идентификатор клиента
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Инициалы клиента
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Заморожен ли счет клиента
    /// </summary>
    public bool Frozen { get; set; }

    /// <summary>
    /// Список счетов, которые относятся к клиенту
    /// </summary>
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}