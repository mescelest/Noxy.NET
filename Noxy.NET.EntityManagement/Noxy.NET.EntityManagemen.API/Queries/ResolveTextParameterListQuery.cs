using MediatR;

namespace Noxy.NET.EntityManagement.API.Queries;

public record ResolveTextParameterListQuery(IEnumerable<string> ParameterKeys) : IRequest<Dictionary<string, string>>;
