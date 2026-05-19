using Mediator;
using Noxy.NET.EntityManagement.API.Queries.Data;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Handlers.Data;

public class HandlerDataParameterStyleResolveList(IParameterService serviceParameter) : IQueryHandler<QueryDataParameterStyleResolveList, ResponseDataParameterStyleResolveList>
{
    public ValueTask<ResponseDataParameterStyleResolveList> Handle(QueryDataParameterStyleResolveList request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new ResponseDataParameterStyleResolveList
        {
            Value = request.SchemaIdentifierList.ToDictionary(x => x, y => serviceParameter.TryGetParameterStyle(y, out EntityDataParameterStyle? parameter) ? parameter.Value : null)
        });
    }
}
