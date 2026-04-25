using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Element;

public class CommandSchemaElementCreate(RequestSchemaElementCreate request) : IRequest<ResponseSchemaElementCreate>
{
    public Guid? SchemaID { get; } = request.SchemaID;
    public string SchemaIdentifier { get; } = request.SchemaIdentifier;
    public string Name { get; } = request.Name;
    public string? Note { get; } = request.Note;
    public Guid TitleParameterTextID { get; } = request.TitleParameterTextID;
    public Guid? DescriptionParameterTextID { get; } = request.DescriptionParameterTextID;
}
