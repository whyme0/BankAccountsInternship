using Api.Presentation;
// ReSharper disable UnusedMemberInSuper.Global

namespace Api.Abstractions;

/// <summary>
/// Единственный объект, который должен быть возвращен endpoint'ом
/// </summary>
public interface IMbResult
{
    /// <summary>
    /// Список ошибок
    /// </summary>
    public List<MbError>? MbError { get; set; }
    
    /// <summary>
    /// Http статус ответа
    /// </summary>
    public int StatusCode { get; set; }
}