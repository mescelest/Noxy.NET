using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Entities.Data;

public class EntityDataPropertyBoolean : EntityDataProperty
{
    public required bool? Value { get; set; }
}
