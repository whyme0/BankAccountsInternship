// ReSharper disable UnusedAutoPropertyAccessor.Global
using Api.Abstractions;

namespace Api.Presentation;

/// <summary>
/// Единственный объект, который должен быть возвращен данным endpoint'ом
/// </summary>
public class MbResult<T> : IMbResult
{
    /// <summary>
    /// Объект, возвращаемый методом контроллера
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// Http статус ответа
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Список ошибок
    /// </summary>
    public List<MbError>? MbError { get; set; }
}

/// <summary>
/// Единственный объект, который должен быть возвращен endpoint'ом
/// </summary>
public class MbResult : IMbResult
{
    /// <summary>
    /// Http статус ответа
    /// </summary>
    public int StatusCode { get; set; }
    /// <summary>
    /// Список ошибок
    /// </summary>
    public List<MbError>? MbError { get; set; }
}