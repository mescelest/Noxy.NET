using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Commands.Schema;

public class CommandSchemaElementCreate(RequestSchemaElementCreate request) : IRequest<ResponseSchemaElementCreate>
{
    public Guid? SchemaID { get; set; } = request.SchemaID;
    public string SchemaIdentifier { get; set; } = request.SchemaIdentifier;
    public string Name { get; set; } = request.Name;
    public string Note { get; set; } = request.Note;
    public Guid TitleParameterTextID { get; set; } = request.TitleParameterTextID;
    public Guid DescriptionParameterTextID { get; set; } = request.DescriptionParameterTextID;
}
