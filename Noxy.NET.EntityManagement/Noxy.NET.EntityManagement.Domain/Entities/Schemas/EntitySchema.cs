using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchema : BaseEntity, ISchemaMetadata
{
    [DisplayName(TextConstants.LabelFormName)]
    [Description(TextConstants.HelpFormName)]
    public required string Name { get; set; }

    [DisplayName(TextConstants.LabelFormNote)]
    [Description(TextConstants.HelpFormNote)]
    public string Note { get; set; } = string.Empty;

    public required bool IsActive { get; set; }
    public required DateTime? TimeActivated { get; set; }

    public List<EntitySchemaContext>? ContextList { get; set; }
    public List<EntitySchemaParameter.Discriminator>? ParameterList { get; set; }
    public List<EntitySchemaElement>? ElementList { get; set; }
    public List<EntitySchemaProperty.Discriminator>? PropertyList { get; set; }
}
