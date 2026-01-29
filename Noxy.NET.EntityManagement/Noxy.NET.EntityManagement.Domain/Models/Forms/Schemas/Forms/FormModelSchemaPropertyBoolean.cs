using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas.Forms;

public class FormModelSchemaPropertyBoolean(EntitySchemaPropertyBoolean? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    [JsonConstructor]
    public FormModelSchemaPropertyBoolean() : this(null)
    {
    }

    public override string APIEndpoint => "Schema/Property/Boolean";
    public override HttpMethod HttpMethod => HttpMethod.Post;
}
