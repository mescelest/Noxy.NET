using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Entities.Data;

public class EntityDataPropertyDateTime : EntityDataProperty
{
    public required DateTime? Value { get; set; }
}
