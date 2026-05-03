using MediatR;
using Noxy.NET.EntityManagement.API.Queries.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyFind(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QuerySchemaPropertyFind, ResponseSchemaPropertyFind>
{
    public async Task<ResponseSchemaPropertyFind> Handle(QuerySchemaPropertyFind request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntitySchemaProperty.Discriminator result = await uow.Schema.GetSchemaPropertyByID(request.ID);

        return new() { Value = result };
    }
}
