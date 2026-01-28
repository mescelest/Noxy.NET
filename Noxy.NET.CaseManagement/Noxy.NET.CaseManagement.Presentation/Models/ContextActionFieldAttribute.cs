using Noxy.NET.CaseManagement.Domain.Enums;
using Noxy.NET.CaseManagement.Domain.Models;

namespace Noxy.NET.CaseManagement.Presentation.Models;

public class ContextActionFieldAttribute(StateActionFieldAttribute state)
{
    public bool IsList { get; private set; } = state.IsList;
    public int Order { get; private set; } = state.Order;
    public AttributeTypeEnum Type { get; private set; } = state.Type;
    public IEnumerable<object?> ValueList { get; private set; } = state.Value.Select(x => x.GetValue());

    public ContextActionFieldAttribute Apply(StateActionFieldAttribute state)
    {
        IsList = state.IsList;
        Order = state.Order;
        Type = state.Type;
        ValueList = state.Value.Select(x => x.GetValue());

        return this;
    }
}
