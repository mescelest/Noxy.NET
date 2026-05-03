using MediatR;
using Noxy.NET.EntityManagement.API.Queries.Schema.Property;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;
using Noxy.NET.Extensions;

namespace Noxy.NET.EntityManagement.API.Handlers.Schema.Property;

public class HandlerSchemaPropertyCount(IUnitOfWorkFactory serviceUoWFactory) : IRequestHandler<QuerySchemaPropertyCount, ResponseSchemaPropertyCount>
{
    public async Task<ResponseSchemaPropertyCount> Handle(QuerySchemaPropertyCount request, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        int result = await uow.Schema.GetSchemaPropertyCount(new()
        {
            SchemaID = request.SchemaID ?? await uow.Schema.GetCurrentSchemaID(),
            Search = request.Search?.ToEscapedSqlLike(),
            PropertyType = request.PropertyType,
        });

        return new() { Value = result };
    }
}
