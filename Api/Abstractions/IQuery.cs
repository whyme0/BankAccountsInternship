using MediatR;

namespace Api.Abstractions
{
    public interface IQuery<out TResponse> : IRequest<TResponse>
    {
    }
}
