// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Api.Features.Accounts;

/// <summary>
/// Представляет поля, которые доступны при обновлении счета
/// </summary>
public class UpdateAccountDto
{
    /// <summary>
    /// Процентная ставка
    /// </summary>
    public decimal? InterestRate { get; set; }
    /// <summary>
    /// Дата закрытия счета
    /// </summary>
    public DateTime? ClosedDate { get; set; }
}