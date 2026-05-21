using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Data;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterTextResolve(IParameterService serviceParameter) : IQueryHandler<QueryDataParameterTextResolve, ResponseDataParameterTextResolve>
{
    public ValueTask<ResponseDataParameterTextResolve> Handle(QueryDataParameterTextResolve request, CancellationToken cancellationToken)
    {
        string? result = serviceParameter.TryGetParameterText(request.SchemaIdentifier, out EntityDataParameterText? parameter) ? parameter.Value : null;
        return ValueTask.FromResult(new ResponseDataParameterTextResolve(result));
    }
}
