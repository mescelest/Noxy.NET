using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Context;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.API.Commands.Schema;

public class CommandSchemaContextCreate(RequestSchemaContextCreate request) : IRequest<ResponseSchemaContextCreate>
{
    public Guid? SchemaID { get; } = request.SchemaID;
    public string SchemaIdentifier { get; } = request.SchemaIdentifier;
    public string Name { get; } = request.Name;
    public string? Note { get; } = request.Note;
    public Guid TitleParameterTextID { get; } = request.TitleParameterTextID;
    public Guid? DescriptionParameterTextID { get; } = request.DescriptionParameterTextID;
}
