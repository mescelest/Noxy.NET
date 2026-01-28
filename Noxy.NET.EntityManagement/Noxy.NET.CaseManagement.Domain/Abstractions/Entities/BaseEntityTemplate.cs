using System.ComponentModel;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Domain.Interfaces;

namespace Noxy.NET.CaseManagement.Domain.Abstractions.Entities;

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
