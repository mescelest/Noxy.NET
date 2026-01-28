using System.ComponentModel;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Domain.Abstractions.Entities;

public class BaseEntitySchemaComponent : BaseEntitySchema
{
    [DisplayName(TextConstants.LabelFormTitle)]
    [Description(TextConstants.HelpFormTitle)]
    public EntitySchemaDynamicValue.Discriminator? TitleDynamic { get; set; }
    public required Guid TitleDynamicID { get; set; }
    
    [DisplayName(TextConstants.LabelFormDescription)]
    [Description(TextConstants.HelpFormDescription)]
    public EntitySchemaDynamicValue.Discriminator? DescriptionDynamic { get; set; }
    public required Guid? DescriptionDynamicID { get; set; }
}
