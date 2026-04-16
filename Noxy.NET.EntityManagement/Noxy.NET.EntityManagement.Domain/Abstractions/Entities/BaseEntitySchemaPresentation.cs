using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Abstractions.Entities;

public class BaseEntitySchemaPresentation : BaseEntitySchema
{
    [DisplayName(TextConstants.LabelFormTitle)]
    [Description(TextConstants.HelpFormTitle)]
    public EntitySchemaParameterText? TitleTextParameter { get; set; }
    public Guid TitleTextParameterID { get; set; }

    [DisplayName(TextConstants.LabelFormDescription)]
    [Description(TextConstants.HelpFormDescription)]
    public EntitySchemaParameterText? DescriptionTextParameter { get; set; }
    public Guid? DescriptionTextParameterID { get; set; }
}
