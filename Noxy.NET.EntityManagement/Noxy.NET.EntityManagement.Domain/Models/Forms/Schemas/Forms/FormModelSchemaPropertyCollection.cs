using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas.Forms;

public class FormModelSchemaPropertyCollection(EntitySchemaPropertyCollection? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    [JsonConstructor]
    public FormModelSchemaPropertyCollection() : this(null)
    {
    }

    public override string APIEndpoint => "Schema/Property/Boolean";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    public List<HasProperty>? PropertyList { get; set; } = entity?.PropertyList?.Select(x => new HasProperty(x)).ToList();

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
