using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Context;

public class CommandSchemaContextClone(Guid id) : IRequest<ResponseSchemaContextClone>
{
    public Guid ID { get; } = id;
}
