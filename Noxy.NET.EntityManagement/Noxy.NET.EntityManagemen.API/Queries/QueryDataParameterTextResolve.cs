using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QueryDataParameterTextResolve(string identifier) : IRequest<ResponseDataParameterTextResolve>
{
    public string Identifier { get; } = identifier;
}
