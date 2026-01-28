using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas.Forms;

public class FormModelSchemaElement(EntitySchemaElement? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    [JsonConstructor]
    public FormModelSchemaElement() : this(null)
    {
    }

    public override string APIEndpoint => "Schema/Element";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    public List<HasProperty>? PropertyList { get; set; } = entity?.PropertyList?.Select(x => new HasProperty(x)).ToList();

    public class HasProperty : RelationOrdinal
    {
        [SetsRequiredMembers]
        public HasProperty(EntityJunctionSchemaElementHasProperty? entity = null)
        {
            ID = entity?.ID ?? Guid.Empty;
            RelationID = entity?.RelationID ?? Guid.Empty;
            Order = entity?.Order ?? 0;
        }

        [JsonConstructor]
        public HasProperty()
        {
        }
    }
}
