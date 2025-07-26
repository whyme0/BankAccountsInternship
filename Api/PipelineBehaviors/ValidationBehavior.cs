using FluentValidation;
using MediatR;

namespace Api.PipelineBehaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Если для данного запроса нет валидаторов, просто продолжаем выполнение
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        // Выполняем все валидаторы для данного запроса
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Собираем все ошибки валидации
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // Если есть ошибки, выбрасываем исключение
        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        // Если ошибок нет, передаем управление следующему в цепочке (обработчику команды)
        return await next(cancellationToken);
    }
}