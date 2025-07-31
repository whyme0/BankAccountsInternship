using Api.Presentation;
// ReSharper disable UnusedMemberInSuper.Global

namespace Api.Abstractions;

public interface IMbResult
{
    public List<MbError>? MbError { get; set; }
    public int StatusCode { get; set; }
}