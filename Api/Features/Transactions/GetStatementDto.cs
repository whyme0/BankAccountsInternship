// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Api.Features.Transactions;

/// <summary>
/// Объект, представляющий параметры выписки счета
/// </summary>
public class GetStatementDto
{
    /// <summary>
    /// Нижняя граница даты
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Верхняя граница даты
    /// </summary>
    public DateTime EndDate { get; set; }
}