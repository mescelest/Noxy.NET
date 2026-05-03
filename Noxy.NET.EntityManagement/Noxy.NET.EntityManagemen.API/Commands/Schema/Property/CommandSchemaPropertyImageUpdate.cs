using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Property;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Property;

public class CommandSchemaPropertyImageUpdate(Guid id, RequestSchemaPropertyImageUpdate request) : IRequest<ResponseSchemaPropertyImageUpdate>
{
    public Guid ID { get; } = id;
    public string SchemaIdentifier { get; } = request.SchemaIdentifier;
    public string Name { get; } = request.Name;
    public string Note { get; } = request.Note;
    public int? Weight { get; } = request.Weight;
    public string AllowedExtensions { get; set; } = request.AllowedExtensions;
    public Guid TitleParameterTextID { get; } = request.TitleParameterTextID;
    public Guid? DescriptionParameterTextID { get; } = request.DescriptionParameterTextID;
}
