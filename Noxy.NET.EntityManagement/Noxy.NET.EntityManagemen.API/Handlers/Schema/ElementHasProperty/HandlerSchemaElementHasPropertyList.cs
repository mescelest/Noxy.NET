using MediatR;
using Noxy.NET.EntityManagement.API.Queries.Schema.ElementHasProperty;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.ElementHasProperty;

public class HandlerSchemaElementHasPropertyList(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QuerySchemaElementHasPropertyList, ResponseSchemaElementHasPropertyList>
{
    public async Task<ResponseSchemaElementHasPropertyList> Handle(QuerySchemaElementHasPropertyList request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntitySchemaElementHasProperty> result = await uow.Schema.GetSchemaElementHasPropertyList(new()
        {
            SchemaPropertyID = request.SchemaPropertyID,
            SchemaElementID = request.SchemaElementID,
        });

        return new() { Value = result };
    }
}
