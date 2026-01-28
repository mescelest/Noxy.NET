using Noxy.NET.CaseManagement.Domain.Entities.Data.Discriminators;

namespace Noxy.NET.CaseManagement.Domain.Entities.Data;

public class EntityDataPropertyDateTime : EntityDataProperty
{
    public required DateTime? Value { get; set; }
}
