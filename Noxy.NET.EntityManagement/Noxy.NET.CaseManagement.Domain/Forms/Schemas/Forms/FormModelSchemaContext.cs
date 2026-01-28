using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

public class FormModelSchemaContext(EntitySchemaContext? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    public override string APIEndpoint => "Schema/Context";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    public List<HasAction>? ActionList { get; set; } = entity?.ActionList?.Select(x => new HasAction(x)).ToList();

    public List<HasElement>? ElementList { get; set; } = entity?.ElementList?.Select(x => new HasElement(x)).ToList();


    [JsonConstructor]
    public FormModelSchemaContext() : this(null)
    {
    }

    public class HasAction : RelationOrdinal
    {
        [JsonConstructor]
        public HasAction()
        {
        }

        [SetsRequiredMembers]
        public HasAction(EntityJunctionSchemaContextHasAction model)
        {
            ID = model.ID;
            RelationID = model.RelationID;
            Order = model.Order;
        }
    }

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
