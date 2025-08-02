// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Api.Presentation;

/// <summary>
/// Представляет собой ошибки возникнувшие в обработки запроса сервером
/// </summary>
public class MbError
{
    /// <summary>
    /// Может быть общим ключевым словом (например, resource) или указывать на поле тела запроса, которое вызвало ошибку
    /// </summary>
    public string PropertyName { get; set; } = null!;
    
    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public string ErrorMessage { get; set; } = null!;
}