using Noxy.NET.CaseManagement.Domain.Enums;
using Noxy.NET.Models;

namespace Noxy.NET.CaseManagement.Domain.Models;

public class StateActionFieldAttribute
{
    public required bool IsList { get; set; }
    public required int Order { get; set; }
    public required AttributeTypeEnum Type { get; set; }
    public required List<JsonProperty> Value { get; set; }
}
