using Mediator;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Property;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.Property;

public class QuerySchemaPropertyList(RequestSchemaPropertyList request) : IQuery<ResponseSchemaPropertyList>
{
    public Guid? SchemaID { get; } = request.SchemaID;
    public string? Search { get; } = request.Search;
    public IReadOnlySet<string>? PropertyType { get; } = request.PropertyType;
    public int? PageNumber { get; } = request.PageNumber;
    public int? PageSize { get; } = request.PageSize;
}
