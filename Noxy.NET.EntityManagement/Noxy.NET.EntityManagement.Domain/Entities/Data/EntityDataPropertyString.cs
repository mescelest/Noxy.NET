using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Entities.Data;

public class EntityDataPropertyString : EntityDataProperty
{
    public required string? Value { get; set; }
}
