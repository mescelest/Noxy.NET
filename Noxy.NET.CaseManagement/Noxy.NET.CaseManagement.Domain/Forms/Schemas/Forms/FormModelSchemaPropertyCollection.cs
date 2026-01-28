using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

public class FormModelSchemaPropertyCollection(EntitySchemaPropertyCollection? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    public override string APIEndpoint => "Schema/Property/Boolean";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    public List<HasProperty>? PropertyList { get; set; } = entity?.PropertyList?.Select(x => new HasProperty(x)).ToList();

    [JsonConstructor]
    public FormModelSchemaPropertyCollection() : this(null)
    {
    }

    public class HasProperty : RelationOrdinal
    {
        [JsonConstructor]
        public HasProperty()
        {
        }

        [SetsRequiredMembers]
        public HasProperty(EntityJunctionSchemaPropertyCollectionHasProperty model)
        {
            ID = model.ID;
            RelationID = model.RelationID;
            Order = model.Order;
        }
    }
}
