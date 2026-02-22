using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Commands.Schema;

public class CommandSchemaCreate(RequestSchemaCreate request) : IRequest<ResponseSchemaCreate>
{
    public string Name { get; set; } = request.Name;
    public string Note { get; set; } = request.Note;
}
