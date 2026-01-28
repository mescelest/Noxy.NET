using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas.Forms;

public class FormModelSchemaDynamicValueSystemParameter(EntitySchemaDynamicValueSystemParameter? entity = null) : BaseFormModelEntitySchemaDynamicValueParameter(entity)
{
    [JsonConstructor]
    public FormModelSchemaDynamicValueSystemParameter() : this(null)
    {
    }

    public override string APIEndpoint => "Schema/DynamicValue/SystemParameter";
    public override HttpMethod HttpMethod => HttpMethod.Post;
}
