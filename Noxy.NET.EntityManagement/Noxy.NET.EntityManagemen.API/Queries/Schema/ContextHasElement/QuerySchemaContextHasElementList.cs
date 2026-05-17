using Mediator;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.ContextHasElement;

public class QuerySchemaContextHasElementList(RequestSchemaContextHasElementList request) : IQuery<ResponseSchemaContextHasElementList>
{
    public Guid? SchemaContextID { get; } = request.SchemaContextID;
    public Guid? SchemaElementID { get; } = request.SchemaElementID;
}
