using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Data;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterSystemResolve(IParameterService serviceParameter) : IQueryHandler<QueryDataParameterSystemResolve, ResponseDataParameterSystemResolve>
{
    public ValueTask<ResponseDataParameterSystemResolve> Handle(QueryDataParameterSystemResolve request, CancellationToken cancellationToken)
    {
        string? result = serviceParameter.TryGetParameterSystem(request.SchemaIdentifier, out EntityDataParameterSystem? parameter) ? parameter.Value : null;
        return ValueTask.FromResult(new ResponseDataParameterSystemResolve(result));
    }
}
