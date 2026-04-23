using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Commands.Schema;

public class CommandSchemaClone(Guid id) : IRequest<ResponseSchemaClone>
{
    public Guid ID { get; } = id;
}
