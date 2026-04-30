using MediatR;
using Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Parameter;

public class HandlerSchemaParameterClone(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<CommandSchemaParameterClone, ResponseSchemaParameterClone>
{
    public async Task<ResponseSchemaParameterClone> Handle(CommandSchemaParameterClone request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaParameter.Discriminator result = await uow.Schema.CloneSchemaParameter(request.ID);

        await uow.Commit();

        return new() { Value = result };
    }
}
