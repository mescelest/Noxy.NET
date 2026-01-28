using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Domain.Abstractions.Entities;

public abstract class BaseEntityTemplate : BaseEntity, IOrderedEntity
{
    [DisplayName(TextConstants.LabelFormName)]
    [Description(TextConstants.HelpFormName)]
    public required string Name { get; set; }

    [DisplayName(TextConstants.LabelFormNote)]
    [Description(TextConstants.HelpFormNote)]
    public required string Note { get; set; }

    [DisplayName(TextConstants.LabelFormOrder)]
    [Description(TextConstants.HelpFormOrder)]
    public required int Order { get; set; }

    public override string ToString()
    {
        return Name;
    }
}
