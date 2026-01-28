using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

public class FormModelSchemaDynamicValueSystemParameter(EntitySchemaDynamicValueSystemParameter? entity = null) : BaseFormModelEntitySchemaDynamicValueParameter(entity)
{
    public override string APIEndpoint => "Schema/DynamicValue/SystemParameter";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [JsonConstructor]
    public FormModelSchemaDynamicValueSystemParameter() : this(null)
    {
    }
}
