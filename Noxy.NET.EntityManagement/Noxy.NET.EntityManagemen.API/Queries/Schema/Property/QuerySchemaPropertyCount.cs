using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Property;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.Property;

public class QuerySchemaPropertyCount(RequestSchemaPropertyCount request) : IRequest<ResponseSchemaPropertyCount>
{
    public Guid? SchemaID { get; } = request.SchemaID;
    public string? Search { get; } = request.Search;
    public IReadOnlySet<string>? PropertyType { get; } = request.PropertyType;
}
