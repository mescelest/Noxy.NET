using Noxy.NET.CaseManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.CaseManagement.Domain.Entities.Data;

public class EntityDataPropertyString : EntityDataProperty
{
    public required string? Value { get; set; }
}
