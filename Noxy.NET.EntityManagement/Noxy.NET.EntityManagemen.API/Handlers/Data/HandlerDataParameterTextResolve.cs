using MediatR;
using Noxy.NET.EntityManagement.API.Queries.Data;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterTextResolve(IParameterService serviceParameter) : IRequestHandler<QueryDataParameterTextResolve, ResponseDataParameterTextResolve>
{
    public Task<ResponseDataParameterTextResolve> Handle(QueryDataParameterTextResolve request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ResponseDataParameterTextResolve
        {
            Value = serviceParameter.TryGetParameterText(request.Identifier, out EntityDataParameterText? parameter) ? parameter.Value : "[MISSING]"
        });
    }
}
