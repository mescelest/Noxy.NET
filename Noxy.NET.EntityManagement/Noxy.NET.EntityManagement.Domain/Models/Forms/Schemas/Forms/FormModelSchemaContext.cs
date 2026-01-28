using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas.Forms;

public class FormModelSchemaContext(EntitySchemaContext? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    [JsonConstructor]
    public FormModelSchemaContext() : this(null)
    {
    }

    public override string APIEndpoint => "Schema/Context";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    public List<HasElement>? ElementList { get; set; } = entity?.ElementList?.Select(x => new HasElement(x)).ToList();

    public class HasElement : RelationOrdinal
    {
        [JsonConstructor]
        public HasElement()
        {
        }

        [SetsRequiredMembers]
        public HasElement(EntityJunctionSchemaContextHasElement model)
        {
            ID = model.ID;
            RelationID = model.RelationID;
            Order = model.Order;
        }
    }
}
