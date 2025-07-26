using FluentValidation;
using MediatR;

namespace Api.PipelineBehaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Если для данного запроса нет валидаторов, просто продолжаем выполнение
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            // Выполняем все валидаторы для данного запроса
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

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
            return await next();
        }
    }
}
