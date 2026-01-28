using Noxy.NET.Models;

namespace Noxy.NET.CaseManagement.Domain.Models;

public class StateActionField
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string[]? ErrorList { get; init; }
    public required bool IsActive { get; init; }
    public required Dictionary<string, StateActionFieldAttribute> AttributeCollection { get; init; }
    public required JsonProperty Value { get; init; }
}
