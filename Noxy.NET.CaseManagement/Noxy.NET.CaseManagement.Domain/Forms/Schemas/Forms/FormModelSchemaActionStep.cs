using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

public class FormModelSchemaActionStep(EntitySchemaActionStep? entity = null) : BaseFormModelEntitySchemaComponent(entity)
{
    public override string APIEndpoint => "Schema/ActionStep";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    [DisplayName(TextConstants.LabelFormIsRepeatable)]
    [Description(TextConstants.HelpFormIsRepeatable)]
    public bool IsRepeatable { get; set; }

    public List<HasActionInput>? ActionInputList { get; set; } = entity?.ActionInputList?.Select(x => new HasActionInput(x)).ToList();

    [JsonConstructor]
    public FormModelSchemaActionStep() : this(null)
    {
    }

    public class HasActionInput : RelationOrdinal
    {
        [JsonConstructor]
        public HasActionInput()
        {
        }

        [SetsRequiredMembers]
        public HasActionInput(EntityJunctionSchemaActionStepHasActionInput model)
        {
            ID = model.ID;
            RelationID = model.RelationID;
            Order = model.Order;
        }
    }
}
