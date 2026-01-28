using Noxy.NET.CaseManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.CaseManagement.Domain.Entities.Data;

public class EntityDataPropertyBoolean : EntityDataProperty
{
    public required bool? Value { get; set; }
}
