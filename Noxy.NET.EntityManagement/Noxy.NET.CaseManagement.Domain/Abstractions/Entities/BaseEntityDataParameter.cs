namespace Noxy.NET.CaseManagement.Domain.Abstractions.Entities;

public abstract class BaseEntityDataParameter : BaseEntityData
{
    public required string Value { get; set; }
    public required DateTime? TimeApproved { get; set; }
    public required DateTime TimeEffective { get; set; }
}
