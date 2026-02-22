using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema;

public class HandlerSchemaCreate(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaCreate, ResponseSchemaCreate>
{
    public async Task<ResponseSchemaCreate> Handle(CommandSchemaCreate request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        // List<EntitySchemaParameter.Discriminator> result = await uow.Schema.Create(request);

        return new() { Value = null };
    }
}
