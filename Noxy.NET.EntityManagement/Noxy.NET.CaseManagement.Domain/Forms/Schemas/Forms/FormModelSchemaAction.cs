using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

public class FormModelSchemaAction(EntitySchemaAction? entity = null) : BaseFormModelEntitySchemaComponent(entity)
{
    public override string APIEndpoint => "Schema/Action";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    [DisplayName(TextConstants.LabelFormIsRepeatable)]
    [Description(TextConstants.HelpFormIsRepeatable)]
    public List<HasActionStep>? ActionStepList { get; set; } = entity?.ActionStepList?.Select(x => new HasActionStep(x)).ToList();

    [JsonConstructor]
    public FormModelSchemaAction() : this(null)
    {
    }

    public class HasActionStep : RelationOrdinal
    {
        [JsonConstructor]
        public HasActionStep()
        {
        }

        [SetsRequiredMembers]
        public HasActionStep(EntityJunctionSchemaActionHasActionStep model)
        {
            ID = model.ID;
            RelationID = model.RelationID;
            Order = model.Order;
        }
    }
}
