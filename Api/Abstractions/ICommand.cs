using MediatR;

namespace Api.Abstractions;

public interface ICommand<out TResponse> : IRequest<TResponse>;