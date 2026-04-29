using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Context;

public class CommandSchemaContextDelete(Guid id) : IRequest<ResponseSchemaContextDelete>
{
    public Guid ID { get; } = id;
}
