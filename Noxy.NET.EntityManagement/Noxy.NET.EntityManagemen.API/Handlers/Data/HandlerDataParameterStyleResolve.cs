using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Data;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterStyleResolve(IParameterService serviceParameter) : IQueryHandler<QueryDataParameterStyleResolve, ResponseDataParameterStyleResolve>
{
    public ValueTask<ResponseDataParameterStyleResolve> Handle(QueryDataParameterStyleResolve request, CancellationToken cancellationToken)
    {
        string? result = serviceParameter.TryGetParameterStyle(request.SchemaIdentifier, out EntityDataParameterStyle? parameter) ? parameter.Value : null;
        return ValueTask.FromResult(new ResponseDataParameterStyleResolve(result));
    }
}
