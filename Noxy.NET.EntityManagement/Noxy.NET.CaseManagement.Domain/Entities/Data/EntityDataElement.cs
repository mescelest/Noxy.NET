using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.CaseManagement.Domain.Entities.Data;

public class EntityDataElement : BaseEntityData
{
    public List<EntityDataProperty.Discriminator>? PropertyList { get; set; }
}
