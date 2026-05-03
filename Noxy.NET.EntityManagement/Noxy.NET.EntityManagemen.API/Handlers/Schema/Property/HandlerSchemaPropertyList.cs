using MediatR;
using Noxy.NET.EntityManagement.API.Queries.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyList(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QuerySchemaPropertyList, ResponseSchemaPropertyList>
{
    public async Task<ResponseSchemaPropertyList> Handle(QuerySchemaPropertyList request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        List<EntitySchemaProperty.Discriminator> result = await uow.Schema.GetSchemaPropertyList(new()
        {
            SchemaID = request.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            Search = request.Search?.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_"),
            PropertyType = request.PropertyType,
            PageSize = request.PageSize ?? 10,
            PageNumber = request.PageNumber ?? 0,
        });

        return new() { Value = result };
    }
}
