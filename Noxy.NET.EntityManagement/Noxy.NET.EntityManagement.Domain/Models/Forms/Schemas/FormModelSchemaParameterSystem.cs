using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas;

public class FormModelSchemaParameterSystem(EntitySchemaParameterSystem? entity = null) : BaseFormModelEntitySchemaParameter(entity)
{
    [JsonConstructor]
    public FormModelSchemaParameterSystem() : this(null)
    {
    }

    public override string APIEndpoint => "Schema/Parameter/System";
    public override HttpMethod HttpMethod => HttpMethod.Post;
}
