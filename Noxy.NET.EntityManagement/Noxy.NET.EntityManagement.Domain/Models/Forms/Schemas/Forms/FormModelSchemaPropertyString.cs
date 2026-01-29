using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas.Forms;

public class FormModelSchemaPropertyString(EntitySchemaPropertyString? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    [JsonConstructor]
    public FormModelSchemaPropertyString() : this(null)
    {
    }

    public override string APIEndpoint => "Schema/Property/String";
    public override HttpMethod HttpMethod => HttpMethod.Post;
}
