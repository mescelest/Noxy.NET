using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

public class FormModelSchemaElement(EntitySchemaElement? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    public override string APIEndpoint => "Schema/Element";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [JsonConstructor]
    public FormModelSchemaElement() : this(null)
    {
    }

    public List<HasAction>? ActionList { get; set; } = entity?.ActionList?.Select(x => new HasAction(x)).ToList();
    public List<HasProperty>? PropertyList { get; set; } = entity?.PropertyList?.Select(x => new HasProperty(x)).ToList();

    public class HasAction : RelationOrdinal
    {
        public HasAction(EntityJunctionSchemaElementHasAction? entity = null)
        {
            ID = entity?.ID ?? Guid.Empty;
            RelationID = entity?.RelationID ?? Guid.Empty;
            Order = entity?.Order ?? 0;
        }

        [JsonConstructor]
        public HasAction()
        {
        }
    }

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
