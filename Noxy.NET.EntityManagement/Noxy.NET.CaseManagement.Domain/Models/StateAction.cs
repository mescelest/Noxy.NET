namespace Noxy.NET.CaseManagement.Domain.Models;

public class StateAction
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required Dictionary<string, StateActionField> FieldCollection { get; init; }
}
