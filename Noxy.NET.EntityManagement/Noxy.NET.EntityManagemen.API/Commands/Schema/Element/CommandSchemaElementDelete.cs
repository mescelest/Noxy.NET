using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Element;

public class CommandSchemaElementDelete(Guid id) : IRequest<ResponseSchemaElementDelete>
{
    public Guid ID { get; } = id;
}
