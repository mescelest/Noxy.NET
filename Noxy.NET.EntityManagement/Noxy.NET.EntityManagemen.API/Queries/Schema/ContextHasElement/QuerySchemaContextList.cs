using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.ContextHasElement;

public class QuerySchemaContextHasElementList(RequestSchemaContextHasElementList request) : IRequest<ResponseSchemaContextHasElementList>
{
    public Guid? SchemaContextID { get; } = request.SchemaContextID;
    public Guid? SchemaElementID { get; } = request.SchemaElementID;
}
