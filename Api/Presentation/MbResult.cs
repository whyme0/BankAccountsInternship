// ReSharper disable UnusedAutoPropertyAccessor.Global
using Api.Abstractions;

namespace Api.Presentation;

public class MbResult<T> : IMbResult
{
    public T? Value { get; set; }
    public int StatusCode { get; set; }
    public List<MbError>? MbError { get; set; }
}

public class MbResult : IMbResult
{
    public int StatusCode { get; set; }
    public List<MbError>? MbError { get; set; }
}