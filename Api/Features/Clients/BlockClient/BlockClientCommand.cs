using Api.Abstractions;
using MediatR;

namespace Api.Features.Clients.BlockClient
{
    public class BlockClientCommand : ICommand<Unit>
    {
        /// <summary>
        /// Уникальный идентификатор клиента
        /// </summary>
        public Guid Id { get; set; }
    }
}
