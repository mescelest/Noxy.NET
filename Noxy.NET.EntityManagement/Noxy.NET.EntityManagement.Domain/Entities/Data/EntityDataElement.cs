using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Entities.Data;

public class EntityDataElement : BaseEntityData
{
    public List<EntityDataProperty.Discriminator>? PropertyList { get; set; }
}
