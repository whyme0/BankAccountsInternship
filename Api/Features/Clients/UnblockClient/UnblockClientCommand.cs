using Api.Abstractions;
using MediatR;

namespace Api.Features.Clients.UnblockClient
{
    public class UnblockClientCommand : ICommand<Unit>
    {
        /// <summary>
        /// Уникальный идентификатор клиента
        /// </summary>
        public Guid Id { get; set; }
    }
}
