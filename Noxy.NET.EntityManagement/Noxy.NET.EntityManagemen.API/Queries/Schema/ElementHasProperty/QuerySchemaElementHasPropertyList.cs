using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.ElementHasProperty;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.ElementHasProperty;

public class QuerySchemaElementHasPropertyList(RequestSchemaElementHasPropertyList request) : IRequest<ResponseSchemaElementHasPropertyList>
{
    public Guid? SchemaElementID { get; } = request.SchemaElementID;
    public Guid? SchemaPropertyID { get; } = request.SchemaPropertyID;
}
